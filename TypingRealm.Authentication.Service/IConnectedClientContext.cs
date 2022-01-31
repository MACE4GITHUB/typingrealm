using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TypingRealm.Messaging;
using TypingRealm.Profiles;

namespace TypingRealm.Authentication.Service;

public interface IConnectedClientContext
{
    AuthenticatedProfile GetProfile();
    string GetAccessToken();
    void SetAuthenticatedContext(ClaimsPrincipal claimsPrincipal, JwtSecurityToken securityToken);
    void SetConnectedClient(ConnectedClient connectedClient);
}
