using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication.OAuth;

namespace TypingRealm.IdentityServer.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            _ = services.AddIdentityServer(options =>
            {
                // This can be turned off, if we turn off AudienceValidation everywhere as well.
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(e => e.MapControllers());
            app.UseIdentityServer();
        }
    }

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
