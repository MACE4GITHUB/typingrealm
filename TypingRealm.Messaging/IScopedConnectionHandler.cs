using System.Threading;

namespace TypingRealm.Messaging;

/// <summary>
/// ConnectionHandler that creates a new dependency scope tree on
/// <see cref="IConnectionHandler.HandleAsync(IConnection, CancellationToken)"/> call.
/// </summary>
public interface IScopedConnectionHandler : IConnectionHandler
{
}
