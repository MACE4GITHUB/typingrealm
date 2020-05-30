using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Updating
{
    /// <summary>
    /// Uses <see cref="IUpdateFactory"/> to construct an update for the client
    /// and sends it to client's connection.
    /// </summary>
    public sealed class Updater : IUpdater
    {
        private readonly IUpdateFactory _updateFactory;

        public Updater(IUpdateFactory updateFactory)
        {
            _updateFactory = updateFactory;
        }

        public ValueTask SendUpdateAsync(ConnectedClient client, CancellationToken cancellationToken)
        {
            var update = _updateFactory.GetUpdateFor(client.ClientId);

            return client.Connection.SendAsync(update, cancellationToken);
        }
    }
}
