using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging.Client;

namespace TypingRealm.SignalR.Client
{
    public sealed class SignalRClientConnectionFactory : IClientConnectionFactory
    {
        private readonly ISignalRConnectionFactory _factory;
        private readonly IProfileTokenProvider _profileTokenProvider;
        private readonly string _uri;

        public SignalRClientConnectionFactory(
            ISignalRConnectionFactory factory,
            IProfileTokenProvider profileTokenProvider,
            string uri)
        {
            _factory = factory;
            _profileTokenProvider = profileTokenProvider;
            _uri = uri;
        }

        public async ValueTask<ConnectionResource> ConnectAsync(CancellationToken cancellationToken)
        {
            var hub = new HubConnectionBuilder()
                .WithUrl(_uri, options =>
                {
                    options.AccessTokenProvider = async () => await _profileTokenProvider.SignInAsync().ConfigureAwait(false);
                    options.Transports = HttpTransportType.WebSockets;
                })
                .Build();

            await hub.StartAsync(default).ConfigureAwait(false);

            var connection = _factory.CreateProtobufConnectionForClient(hub);

            return new ConnectionResource(connection, async () =>
            {
                await hub.StopAsync().ConfigureAwait(false);
                await hub.DisposeAsync().ConfigureAwait(false);
            });
        }
    }
}
