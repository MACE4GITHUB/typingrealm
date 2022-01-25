using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connecting;

/// <summary>
/// Connection initializer handles client connection during the moment when
/// client is being connected. It serves as ConnectedClient factory,
/// creating the instance of <see cref="ConnectedClient"/> when connection
/// was successful.
/// </summary>
public interface IConnectionInitializer
{
    /// <summary>
    /// This method is called when the client is being connected. It is
    /// responsible for validating the connection using some particular
    /// messaging flow and creating a newly constructed and valid
    /// <see cref="ConnectedClient"/> instance. The implementation can
    /// contain custom logic on waiting and handling of specific messages.
    /// </summary>
    /// <param name="connection">Client's connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connected client instance if connection was successful.
    ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken);
}
