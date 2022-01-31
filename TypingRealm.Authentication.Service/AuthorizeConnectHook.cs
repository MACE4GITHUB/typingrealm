using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Authentication.Service;

public sealed class AuthorizeConnectHook : IConnectHook
{
    private readonly IConnectedClientContext _connectedClientContext;

    public AuthorizeConnectHook(IConnectedClientContext connectedClientContext)
    {
        _connectedClientContext = connectedClientContext;
    }

    public ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
    {
        var profile = _connectedClientContext.GetProfile();
        if (profile.ProfileId != connect.ClientId)
            throw new InvalidOperationException("Different client ID from profile ID is not allowed.");

        // Consider just setting ClientId and not forcing the client to send it.

        return default;
    }
}
