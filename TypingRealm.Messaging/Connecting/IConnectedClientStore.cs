using System.Collections.Generic;

namespace TypingRealm.Messaging.Connecting;

/// <summary>
/// ConnectedClientStore provides access to the pool of currently connected clients.
/// Methods on this store can be called concurrently, implementation should
/// be thread-safe.
/// Implementation should throw <see cref="ClientAlreadyConnectedException"/>
/// when adding a client that has already connected.
/// </summary>
public interface IConnectedClientStore
{
    /// <summary>
    /// Adds connected client to the store if it isn't connected yet.
    /// </summary>
    /// <param name="connectedClient">Connected client.</param>
    /// <exception cref="ClientAlreadyConnectedException">Thrown when trying
    /// to add a client that has already connected (by ClientId).</exception>
    void Add(ConnectedClient connectedClient);

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
