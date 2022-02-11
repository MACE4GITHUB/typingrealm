using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Authentication.Service;

public sealed class UserProfileConnectionInitializer : IConnectionInitializer
{
    private readonly IConnectedClientContext _connectedClientContext;
    private readonly IUpdateDetector _updateDetector;

    public UserProfileConnectionInitializer(
        IConnectedClientContext connectedClientContext,
        IUpdateDetector updateDetector)
    {
        _connectedClientContext = connectedClientContext;
        _updateDetector = updateDetector;
    }

    public ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        var profileId = _connectedClientContext.GetProfile()?.ProfileId;
        if (profileId == null)
            throw new InvalidOperationException("User token does not have a name.");

        var connectedClient = new ConnectedClient(profileId, connection, _updateDetector, Connect.DefaultGroup);
        return new(connectedClient);
    }
}
