using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TypingRealm.RopeWar.TcpServer
{
    public sealed class RopeWarTcpServer : IHostedService
    {
        private readonly TcpServer _tcpServer;

        public RopeWarTcpServer(TcpServer tcpServer)
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
