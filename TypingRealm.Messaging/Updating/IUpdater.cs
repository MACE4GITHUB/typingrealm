using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Updating
{
    /// <summary>
    /// Sends update to client (after the client has been marked for an update).
    /// </summary>
    public interface IUpdater
    {
        /// <summary>
        /// Sends update to client.
        /// </summary>
        /// <param name="client">Connected client.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        ValueTask SendUpdateAsync(ConnectedClient client, CancellationToken cancellationToken);
    }
}
