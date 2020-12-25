using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication;

namespace TypingRealm.Profiles.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/local-token")]
    public sealed class LocalAuthenticationController : ControllerBase
    {
        [HttpPost]
        public TokenResponse GenerateToken(string sub)
        {
            return new TokenResponse
            {
                access_token = LocalAuthentication.GenerateJwtAccessToken(sub)
            };
        }
    }
}
