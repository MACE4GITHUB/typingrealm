using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Handlers
{
    public sealed class AnnounceHandler : IMessageHandler<Announce>
    {
        private readonly IConnectedClientStore _connectedClients;

        public AnnounceHandler(IConnectedClientStore connectedClients)
        {
            _connectedClients = connectedClients;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Announce message, CancellationToken cancellationToken)
        {
            var broadcastMessage = new Announce($"{sender.ClientId} > {message.Message}");
            var groups = sender.Groups;

            return _connectedClients.SendAsync(broadcastMessage, groups, cancellationToken);
        }
    }
}
