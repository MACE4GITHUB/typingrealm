using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TypingRealm.Authentication
{
    public interface IProfileContext
    {
        string GetAccessToken();
        void SetAuthenticatedContext(ClaimsPrincipal claimsPrincipal, JwtSecurityToken securityToken);
    }
}
