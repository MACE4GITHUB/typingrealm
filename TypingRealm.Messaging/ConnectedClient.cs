using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging
{
    /// <summary>
    /// Every live connection has single ConnectedClient instance associated with it.
    /// Changing client's Group will schedule update for all the clients that
    /// are currently in both previous and new group of this client.
    /// </summary>
    public sealed class ConnectedClient
    {
        private readonly IUpdateDetector _updateDetector;
        private string _group;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedClient"/> class.
        /// </summary>
        /// <param name="clientId">Unique connected client identifier. There
        /// should be no any clients connected at the same time with the same
        /// identifier.</param>
        /// <param name="connection">Client's connection.</param>
        /// <param name="group">Messaging group where the client is placed initially.</param>
        /// <param name="updateDetector">Update detector for marking groups for
        /// update when the group changes.</param>
        public ConnectedClient(
            string clientId,
            IConnection connection,
            string group,
            IUpdateDetector updateDetector)
        {
            ClientId = clientId;
            Connection = connection;
            _group = group;
            _updateDetector = updateDetector;
        }

        /// <summary>
        /// Gets unique connected client identifier.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets client's connection.
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        /// Gets or sets messaging group where the client belongs to. Marks
        /// update detector for changes so that both previous and new groups are
        /// updated on the next update cycle.
        /// </summary>
        public string Group
        {
            get => _group;
            set
            {
                if (_group != value)
                {
                    _updateDetector.MarkForUpdate(_group);
                    _group = value;
                    _updateDetector.MarkForUpdate(_group);
                }
            }
        }
    }
}
