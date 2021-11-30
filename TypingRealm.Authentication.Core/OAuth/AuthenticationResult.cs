using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TypingRealm.Authentication.OAuth
{
    public sealed class AuthenticationResult
    {
        public AuthenticationResult(ClaimsPrincipal claimsPrincipal, JwtSecurityToken jwtSecurityToken)
        {
            ClaimsPrincipal = claimsPrincipal;
            JwtSecurityToken = jwtSecurityToken;
        }

        public ClaimsPrincipal ClaimsPrincipal { get; }
        public JwtSecurityToken JwtSecurityToken { get; }
    }
}
