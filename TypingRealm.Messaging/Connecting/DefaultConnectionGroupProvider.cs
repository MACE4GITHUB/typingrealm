using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Connecting;

public sealed class DefaultConnectionGroupProvider : IConnectionGroupProvider
{
    public const string DefaultGroup = "Lobby";

    public ValueTask<IEnumerable<string>> GetGroupsAsync(IConnection connection, CancellationToken cancellationToken)
    {
        return new(new[] { DefaultGroup });
    }
}

public interface IConnectedClientFactory
{
    ValueTask<ConnectedClient> CreateConnectedClientAsync(
        string clientId,
        IConnection connection,
        CancellationToken cancellationToken);
}

public sealed class DefaultConnectedClientFactory : IConnectedClientFactory
{
    private readonly IUpdateDetector _updateDetector;
    private readonly IConnectionGroupProvider _connectionGroupProvider;

    public DefaultConnectedClientFactory(
        IUpdateDetector updateDetector,
        IConnectionGroupProvider connectionGroupProvider)
    {
        _updateDetector = updateDetector;
        _connectionGroupProvider = connectionGroupProvider;
    }

    public async ValueTask<ConnectedClient> CreateConnectedClientAsync(
        string clientId,
        IConnection connection,
        CancellationToken cancellationToken)
    {
        var groups = await _connectionGroupProvider.GetGroupsAsync(connection, cancellationToken)
            .ConfigureAwait(false);

        return new ConnectedClient(clientId, connection, _updateDetector, groups);
    }
}
