using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Hosting
{
    // TODO: Fail healthcheck or stop the whole service if some deployment services have failed.
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
            try
            {
                await deploy.DeployInfrastructureAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                _logger.LogError("Failed to deploy infrastructure from {DeployType} type.", deploy.GetType());
                throw;
            }
        }
    }
}
