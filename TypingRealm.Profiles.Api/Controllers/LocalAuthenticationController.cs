using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication;
using TypingRealm.Authorization;

namespace TypingRealm.Profiles.Api.Controllers
{
    public sealed class TokenRequest
    {
        public string? client_id { get; set; }
        public string? grant_type { get; set; }
    }

    [AllowAnonymous]
    [Route("api/local-auth")]
    public sealed class LocalAuthenticationController : ControllerBase
    {
        [HttpPost]
        [Route("profile-token")]
        public TokenResponse GenerateToken(string sub)
        {
            return new TokenResponse
            {
                access_token = LocalAuthentication.GenerateProfileAccessToken(sub)
            };
        }

        [HttpPost]
        [Route("token")]
        public TokenResponse GenerateToken([FromForm]TokenRequest request)
        {
            if (request?.client_id == null)
                throw new InvalidOperationException("Client id is not supplied in token request.");

            return new TokenResponse
            {
                access_token = LocalAuthentication.GenerateServiceAccessToken(request.client_id)
            };
        }

        [Authorize]
        [ServiceScoped]
        [Route("service")]
        public ActionResult TryServiceToService()
        {
            // Should work only with service-to-service tokens.

            return Ok();
        }
    }
}
