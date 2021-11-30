using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication.Service
{
    public interface IConnectedClientContext
    {
        string GetAccessToken();
        void SetAuthenticatedContext(ClaimsPrincipal claimsPrincipal, JwtSecurityToken securityToken);
        void SetConnectedClient(ConnectedClient connectedClient);
    }
}
