using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication
{
    public sealed class ConnectedClientContext : IConnectedClientContext
    {
        private ConnectedClient? _connectedClient;
        private ClaimsPrincipal? _claimsPrincipal;
        private JwtSecurityToken? _securityToken;

        public string GetAccessToken() => _securityToken?.RawData ?? throw new InvalidOperationException("Access token is not set.");

        public void SetAuthenticatedContext(ClaimsPrincipal claimsPrincipal, JwtSecurityToken securityToken)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            _claimsPrincipal = claimsPrincipal;
            _securityToken = securityToken;
        }

        public void SetConnectedClient(ConnectedClient connectedClient)
        {
            if (connectedClient == null)
                throw new ArgumentNullException(nameof(connectedClient));

            _connectedClient = connectedClient;
        }
    }
}
