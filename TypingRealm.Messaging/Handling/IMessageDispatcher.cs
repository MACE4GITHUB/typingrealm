using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Handling
{
    /// <summary>
    /// Dispatches message to all registered handlers.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatches message to all registered handlers.
        /// </summary>
        /// <param name="sender">ConnectedClient associated with the sender of the message.</param>
        /// <param name="message">Message that needs to be dispatched.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        ValueTask DispatchAsync(ConnectedClient sender, object message, CancellationToken cancellationToken);
    }
}
