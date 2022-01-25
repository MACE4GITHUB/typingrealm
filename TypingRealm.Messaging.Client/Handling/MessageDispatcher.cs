using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client.Handling;

public sealed class MessageDispatcher : IMessageDispatcher
{
    private readonly IMessageHandlerFactory _messageHandlerFactory;

    public MessageDispatcher(IMessageHandlerFactory messageHandlerFactory)
    {
        _messageHandlerFactory = messageHandlerFactory;
    }

    public ValueTask DispatchAsync(object message, CancellationToken cancellationToken)
    {
        return DynamicDispatchAsync((dynamic)message, cancellationToken);
    }

    private ValueTask DynamicDispatchAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        var valueTasks = _messageHandlerFactory.GetHandlersFor<TMessage>()
            .Select(handler => handler.HandleAsync(message, cancellationToken));

        return AsyncHelpers.WhenAll(valueTasks);
    }
}
