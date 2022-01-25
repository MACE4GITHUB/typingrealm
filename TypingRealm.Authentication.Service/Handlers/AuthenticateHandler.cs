using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Authentication.Service.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication.Service.Handlers;

public sealed class AuthenticateHandler : IMessageHandler<Authenticate>
{
    private readonly ITokenAuthenticationService _tokenAuthenticationService;
    private readonly IConnectedClientContext _connectedClientContext;

    public AuthenticateHandler(
        ITokenAuthenticationService tokenAuthenticationService,
        IConnectedClientContext connectedClientContext)
    {
        _tokenAuthenticationService = tokenAuthenticationService;
        _connectedClientContext = connectedClientContext;
    }

    public async ValueTask HandleAsync(ConnectedClient sender, Authenticate message, CancellationToken cancellationToken)
    {
        var authenticationResult = await _tokenAuthenticationService.AuthenticateAsync(message.AccessToken, cancellationToken)
            .ConfigureAwait(false);

        _connectedClientContext.SetAuthenticatedContext(authenticationResult.ClaimsPrincipal, authenticationResult.JwtSecurityToken);
    }
}
