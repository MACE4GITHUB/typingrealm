using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connecting;

public static class ConnectedClientStoreExtensions
{
    /// <summary>
    /// Gets a value indicating whether given client is present in the store
    /// (if it's connected - it will be present in the store).
    /// </summary>
    /// <param name="store">ConnectedClientStore instance.</param>
    /// <param name="clientId">Connected client identity.</param>
    /// <returns>A value indicating whether given client is present in the store.</returns>
    public static bool IsClientConnected(this IConnectedClientStore store, string clientId)
        => store.Find(clientId) != null;

    /// <summary>
    /// Finds all clients that are in given messaging group.
    /// </summary>
    /// <param name="store">ConnectedClientStore instance.</param>
    /// <param name="groups">Collection of messaging groups to search for.</param>
    /// <returns>Collection of clients that are present in requested group.</returns>
    public static IEnumerable<ConnectedClient> FindInGroups(this IConnectedClientStore store, params string[] groups)
        => store.FindInGroups(groups);

    /// <summary>
    /// Sends message to all the clients in specified group.
    /// </summary>
    /// <param name="store">ConnectedClientStore instance.</param>
    /// <param name="message">Message to send to all the clients in given group.</param>
    /// <param name="group">The group to which the message will be sent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public static ValueTask SendAsync(this IConnectedClientStore store, object message, string group, CancellationToken cancellationToken)
        => AsyncHelpers.WhenAll(store.FindInGroups(group)
            .Select(x => x.Connection.SendAsync(message, cancellationToken)));

    /// <summary>
    /// Sends message to all the clients in specified list of groups.
    /// </summary>
    /// <param name="store">ConnectedClientStore instance.</param>
    /// <param name="message">Message to send to all the clients in given group.</param>
    /// <param name="groups">The list of groups to which the message will be sent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public static ValueTask SendAsync(this IConnectedClientStore store, object message, IEnumerable<string> groups, CancellationToken cancellationToken)
        => AsyncHelpers.WhenAll(store.FindInGroups(groups)
            .Select(x => x.Connection.SendAsync(message, cancellationToken)));
}
