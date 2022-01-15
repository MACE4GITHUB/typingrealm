using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Hosting.Deployment
{
    public sealed class InfrastructureDeploymentHostedService : SyncManagedDisposable, IHostedService
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InfrastructureDeploymentHostedService> _logger;

        public InfrastructureDeploymentHostedService(
            IServiceProvider serviceProvider,
            ILogger<InfrastructureDeploymentHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
            using var scope = _serviceProvider.CreateScope();

            var deploymentServices = scope.ServiceProvider.GetServices<IInfrastructureDeploymentService>();

            // Until everything is deployed - host will not start.
            await Task.WhenAll(deploymentServices.Select(
                d => DeployInfrastructureAsync(d, cts.Token)))
                .ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            Dispose();
            return Task.CompletedTask;
        }

        protected override void DisposeManagedResources()
        {
            _cts.Dispose();
        }

        private async Task DeployInfrastructureAsync(IInfrastructureDeploymentService deploy, CancellationToken cancellationToken)
        {
            var i = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await deploy.DeployInfrastructureAsync(cancellationToken)
                        .ConfigureAwait(false);

                    _logger.LogInformation("Successfully deployed infrastructure: {DeployType}.", deploy.GetType());
                    return;
                }
                catch (Exception exception)
                {
                    i++;
                    _logger.LogError(exception, "Failed to deploy infrastructure from {DeployType} type.", deploy.GetType());

                    if (i > 5)
                    {
                        _logger.LogCritical(exception, "Could not deploy infrastructure from {DeployType} type. Host will be shut down.", deploy.GetType());
                        throw;
                    }

                    _logger.LogDebug("Waiting for one minute to retry {DeployType} again.", deploy.GetType());
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
