using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Connecting.Initializers
{
    /// <summary>
    /// Connection initializer that accepts all connections and assigns newly
    /// generated GUID identity to every connected client and default "Lobby" group.
    /// </summary>
    public sealed class AnonymousConnectionInitializer : IConnectionInitializer
    {
        private const string DefaultGroup = "Lobby";
        private readonly IUpdateDetector _updateDetector;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousConnectionInitializer"/> class.
        /// </summary>
        /// <param name="updateDetector">Update detector used when creating new
        /// instances of <see cref="ConnectedClient"/>.</param>
        public AnonymousConnectionInitializer(IUpdateDetector updateDetector)
        {
            _updateDetector = updateDetector;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectedClient"/> instance with new GUID
        /// identity and default "Lobby" group.
        /// </summary>
        /// <param name="connection">Client's connection.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
        {
            var clientId = Guid.NewGuid().ToString();
            var connectedClient = new ConnectedClient(clientId, connection, _updateDetector, DefaultGroup);

            return new ValueTask<ConnectedClient>(connectedClient);
        }
    }
}
