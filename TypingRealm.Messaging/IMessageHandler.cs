using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging;

/// <summary>
/// Handles specific message. There can be multiple handlers for the same
/// message type.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IMessageHandler<in TMessage>
{
    /// <summary>
    /// Handles message received from client.
    /// </summary>
    /// <param name="sender">Client that sent the message.</param>
    /// <param name="message">Message that was sent by client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask HandleAsync(ConnectedClient sender, TMessage message, CancellationToken cancellationToken);
}
