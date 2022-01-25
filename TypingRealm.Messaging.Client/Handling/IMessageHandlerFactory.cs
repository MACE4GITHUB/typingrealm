using System.Collections.Generic;

namespace TypingRealm.Messaging.Client.Handling;

/// <summary>
/// Resolves registered instances of <see cref="IMessageHandler{TMessage}"/>.
/// </summary>
public interface IMessageHandlerFactory
{
    /// <summary>
    /// Resolves all registered instances of <see cref="IMessageHandler{TMessage}"/>.
    /// </summary>
    IEnumerable<IMessageHandler<TMessage>> GetHandlersFor<TMessage>();
}
