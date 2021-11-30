﻿using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication
{
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
}
