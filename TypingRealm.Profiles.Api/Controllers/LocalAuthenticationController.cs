using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication;

namespace TypingRealm.Profiles.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/local-token")]
    public sealed class LocalAuthenticationController : ControllerBase
    {
        [HttpGet]
        public string GenerateToken(string sub)
        {
            return LocalAuthentication.GenerateJwtAccessToken(sub);
        }
    }
}
