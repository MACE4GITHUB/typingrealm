using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Authentication.Service.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Authentication.Service;

public sealed class AuthenticateConnectionInitializer : IConnectionInitializer
{
    private readonly IConnectionInitializer _connectionInitializer;
    private readonly ITokenAuthenticationService _tokenAuthenticationService;
    private readonly IConnectedClientContext _connectedClientContext;
    private readonly IUpdateDetector _updateDetector;

    // TODO: This is a major HACK. Refactor this so that it's configurable.
    // For now EVERYTHING will skip the Connect's ConnectInitializer that also checks all IConnectHook-s.
    // Everything is broken.
    private const bool UseProfileClientId = true;

    public AuthenticateConnectionInitializer(
        IConnectionInitializer connectionInitializer,
        ITokenAuthenticationService tokenAuthenticationService,
        IConnectedClientContext connectedClientContext,
        IUpdateDetector updateDetector)
    {
        _connectionInitializer = connectionInitializer;
        _tokenAuthenticationService = tokenAuthenticationService;
        _connectedClientContext = connectedClientContext;
        _updateDetector = updateDetector;
    }

    public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        if (!(await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false) is Authenticate authenticate))
        {
            await connection.SendAsync(new Disconnected("First message is not a valid Authenticate message."), cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("First message is not a valid Authenticate message.");
        }

        var authenticationResult = await _tokenAuthenticationService.AuthenticateAsync(authenticate.AccessToken, cancellationToken)
            .ConfigureAwait(false);

        _connectedClientContext.SetAuthenticatedContext(authenticationResult.ClaimsPrincipal, authenticationResult.JwtSecurityToken);

        ConnectedClient connectedClient;
        if (!UseProfileClientId)
        {
            connectedClient = await _connectionInitializer.ConnectAsync(connection, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var profileId = authenticationResult.ClaimsPrincipal.Identity?.Name;
            if (profileId == null)
                throw new InvalidOperationException("User token does not have a name.");

            connectedClient = new ConnectedClient(profileId, connection, _updateDetector, Connect.DefaultGroup);
        }

        _connectedClientContext.SetConnectedClient(connectedClient);

        return connectedClient;
    }
}
