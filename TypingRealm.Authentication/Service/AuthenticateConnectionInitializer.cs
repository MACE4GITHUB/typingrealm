using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Authentication.Service.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Authentication.Service
{
    public sealed class AuthenticateConnectionInitializer : IConnectionInitializer
    {
        private readonly IConnectionInitializer _connectionInitializer;
        private readonly ITokenAuthenticationService _tokenAuthenticationService;
        private readonly IConnectedClientContext _connectedClientContext;

        public AuthenticateConnectionInitializer(
            IConnectionInitializer connectionInitializer,
            ITokenAuthenticationService tokenAuthenticationService,
            IConnectedClientContext connectedClientContext)
        {
            _connectionInitializer = connectionInitializer;
            _tokenAuthenticationService = tokenAuthenticationService;
            _connectedClientContext = connectedClientContext;
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

            var connectedClient = await _connectionInitializer.ConnectAsync(connection, cancellationToken)
                .ConfigureAwait(false);

            _connectedClientContext.SetConnectedClient(connectedClient);

            return connectedClient;
        }
    }
}
