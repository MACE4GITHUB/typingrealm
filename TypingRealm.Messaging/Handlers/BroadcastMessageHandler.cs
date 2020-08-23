using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Handlers
{
    public sealed class BroadcastMessageHandler : IMessageHandler<BroadcastMessage>
    {
        private readonly IConnectedClientStore _connectedClients;

        public BroadcastMessageHandler(IConnectedClientStore connectedClients)
        {
            _connectedClients = connectedClients;
        }

        public ValueTask HandleAsync(ConnectedClient sender, BroadcastMessage message, CancellationToken cancellationToken)
        {
            message.SenderId = sender.ClientId;

            var clients = _connectedClients.FindInGroups(sender.Group);

            return AsyncHelpers.WhenAll(clients
                .Except(new[] { sender })
                .Select(c => c.Connection.SendAsync(message, cancellationToken)));
        }
    }
}
