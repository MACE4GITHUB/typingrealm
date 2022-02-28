using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Authentication.Service;

public sealed class UserProfileConnectionInitializer : IConnectionInitializer
{
    private readonly IConnectedClientContext _connectedClientContext;
    private readonly IConnectedClientFactory _connectedClientFactory;

    public UserProfileConnectionInitializer(
        IConnectedClientContext connectedClientContext,
        IConnectedClientFactory connectedClientFactory)
    {
        _connectedClientContext = connectedClientContext;
        _connectedClientFactory = connectedClientFactory;
    }

    public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        var profileId = _connectedClientContext.GetProfile()?.ProfileId;
        if (profileId == null)
            throw new InvalidOperationException("User token does not have a name.");

        var connectedClient = await _connectedClientFactory.CreateConnectedClientAsync(profileId, connection, cancellationToken)
            .ConfigureAwait(false);

        return connectedClient;
    }
}
