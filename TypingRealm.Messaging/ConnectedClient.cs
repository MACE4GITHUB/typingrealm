namespace TypingRealm.Messaging
{
    /// <summary>
    /// Every live connection has single ConnectedClient instance associated with it.
    /// </summary>
    public sealed class ConnectedClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedClient"/> class.
        /// </summary>
        /// <param name="clientId">Unique connected client identifier. There
        /// should be no any clients connected at the same time with the same
        /// identifier.</param>
        /// <param name="connection">Client's connection.</param>
        public ConnectedClient(string clientId, IConnection connection)
        {
            ClientId = clientId;
            Connection = connection;
        }

        /// <summary>
        /// Gets unique connected client identifier.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets client's connection.
        /// </summary>
        public IConnection Connection { get; }
    }
}
