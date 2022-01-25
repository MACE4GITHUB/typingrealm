using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging;

/// <summary>
/// Handles incoming connection.
/// </summary>
public interface IConnectionHandler
{
    /// <summary>
    /// Handles connection. This method may never end. It will continue
    /// executing as long as the connection lives. This method is the entry
    /// point of the main logic of the messaging framework that allows
    /// handling and sending of messages. It should be called after
    /// connection is established and it should be canceled by cancellation
    /// token when the server is going to shut down.
    /// </summary>
    /// <param name="connection">Connection to the client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(IConnection connection, CancellationToken cancellationToken);
}
