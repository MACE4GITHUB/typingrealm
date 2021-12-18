using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication.OAuth;

namespace TypingRealm.IdentityServer.Host
{
    [AllowAnonymous]
    [Route("api/local")]
    public sealed class LocalAuthenticationController : ControllerBase
    {
        private readonly IHostEnvironment _environment;

        public LocalAuthenticationController(IHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [Route("user-token")]
        public ActionResult GenerateToken(string sub)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            if (string.IsNullOrWhiteSpace(sub))
                return BadRequest("Sub (user ID) should not be empty.");

            return Ok(new
            {
                access_token = LocalAuthentication.GenerateProfileAccessToken($"local-auth_{sub}")
            });
        }
    }
}
