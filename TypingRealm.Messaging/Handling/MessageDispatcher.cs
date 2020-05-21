using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Handling
{
    public sealed class MessageDispatcher : IMessageDispatcher
    {
        private readonly IMessageHandlerFactory _messageHandlerFactory;

        public MessageDispatcher(IMessageHandlerFactory messageHandlerFactory)
        {
            _messageHandlerFactory = messageHandlerFactory;
        }

        public ValueTask DispatchAsync(ConnectedClient sender, object message, CancellationToken cancellationToken)
        {
            return DynamicDispatchAsync(sender, (dynamic)message, cancellationToken);
        }

        private ValueTask DynamicDispatchAsync<TMessage>(ConnectedClient sender, TMessage message, CancellationToken cancellationToken)
        {
            var valueTasks = _messageHandlerFactory.GetHandlersFor<TMessage>()
                .Select(handler => handler.HandleAsync(sender, message, cancellationToken));

            return AsyncHelpers.WhenAll(valueTasks);
        }
    }
}
