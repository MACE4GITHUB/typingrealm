using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client;

/// <summary>
/// Handles specific message. There can be multiple handlers for the same
/// message type.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IMessageHandler<in TMessage>
{
    /// <summary>
    /// Handles message received from server.
    /// </summary>
    /// <param name="message">Message that was sent by server.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken);
}
