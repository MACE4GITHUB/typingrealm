using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TypingRealm.Authentication
{
    public sealed class ProfileContext : IProfileContext
    {
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
    }
}
