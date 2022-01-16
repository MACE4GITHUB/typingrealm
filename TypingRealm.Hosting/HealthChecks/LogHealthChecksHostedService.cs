using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Hosting.HealthChecks
{
    public sealed class LogHealthChecksHostedService : AsyncManagedDisposable, IHostedService
    {
        private const string HealthCheckLogMessage = "HealthCheck {HealthStatus} {@HealthCheckReport}";
        private readonly ILogger<LogHealthChecksHostedService> _logger;
        private readonly HealthCheckService _healthCheckService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _logging;

        public LogHealthChecksHostedService(
            ILogger<LogHealthChecksHostedService> logger,
            HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            _logging = Task.Run(async () =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    var report = await _healthCheckService.CheckHealthAsync(cancellationToken)
                        .ConfigureAwait(false);

                    if (report.Status == HealthStatus.Healthy)
                        _logger.LogInformation(HealthCheckLogMessage, report.Status, report);
                    else
                        _logger.LogWarning(HealthCheckLogMessage, report.Status, report);

                    await Task.Delay(TimeSpan.FromSeconds(10))
                        .ConfigureAwait(false);
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            _cts.Cancel();

            if (_logging != null)
                await _logging
                    .SwallowCancellationAsync()
                    .ConfigureAwait(false);
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();

            if (_logging != null)
            {
                await _logging
                    .SwallowCancellationAsync()
                    .HandleExceptionAsync<Exception>(exception =>
                    {
                        _logger.LogError(exception, "Error while disposing of healthchecks logger.");
                    })
                    .ConfigureAwait(false);
            }

            _cts.Dispose();
        }
    }
}
