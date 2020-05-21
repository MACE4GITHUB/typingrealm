using System.Collections.Generic;

namespace TypingRealm.Messaging.Connecting
{
    /// <summary>
    /// ConnectedClientStore provides access to the pool of currently connected clients.
    /// Methods on this store can be called concurrently, implementation should
    /// be thread-safe.
    /// </summary>
    public interface IConnectedClientStore
    {
        /// <summary>
        /// This method serves as a factory for a new ConnectedClient. When client
        /// is connected, this method is called and it should provide atomic
        /// operation of constructing and adding the instance of ConnectedClient
        /// to this store.
        /// Returns null if the client is already present in the store.
        /// </summary>
        /// <param name="clientId">Unique connected client's identity.</param>
        /// <param name="connection">Client's connection.</param>
        /// <param name="group">Messaging group.</param>
        /// <returns>New added ConnectedClient instance or null if the client
        /// was already present in the store.</returns>
        ConnectedClient? Add(string clientId, IConnection connection, string group);

        /// <summary>
        /// Finds a client by its identity, returns null if the client is not found.
        /// </summary>
        /// <param name="clientId">Connected client's identity.</param>
        /// <returns>ConnectedClient instance or null if not found.</returns>
        ConnectedClient? Find(string clientId);

        /// <summary>
        /// Finds all clients that are in given messaging group.
        /// </summary>
        /// <param name="groups">Collection of messaging groups to search for.</param>
        /// <returns>Collection of clients that are present in requested group.</returns>
        IEnumerable<ConnectedClient> FindInGroups(IEnumerable<string> groups);

        /// <summary>
        /// Removes client from the store. Typically this method is called when
        /// the client is disconnected.
        /// </summary>
        /// <param name="clientId">Connected client's identity.</param>
        void Remove(string clientId);
    }
}
