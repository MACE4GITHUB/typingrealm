using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Handlers
{
    public class DisconnectHandler : IMessageHandler<Disconnect>
    {
        private readonly IConnectedClientStore _store;

        public DisconnectHandler(IConnectedClientStore store)
        {
            _store = store;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Disconnect message, CancellationToken cancellationToken)
        {
            _store.Remove(sender.ClientId);

            return sender.Connection.SendAsync(new Disconnected("Requested."), cancellationToken);
        }
    }
}
