using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TypingRealm.Tcp;

namespace TypingRealm.Hosting.Service
{
    public sealed class TcpServerHostedService : IHostedService
    {
        private readonly TcpServer _tcpServer;

        public TcpServerHostedService(TcpServer tcpServer)
        {
            _tcpServer = tcpServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tcpServer.Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _tcpServer.StopAsync().ConfigureAwait(false);
        }
    }
}
