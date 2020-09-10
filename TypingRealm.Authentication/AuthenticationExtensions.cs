using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TypingRealm.Authentication.Adapters;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Authentication
{
    public static class AuthenticationExtensions
    {
        public static void AddTypingRealmAuthentication(this IServiceCollection services)
        {
            // Slash at the end is mandatory - scopes issued by Auth0 have this authority.
            var authority = "https://typingrealm.us.auth0.com/";
            var audience = "https://api.typingrealm.com";

            services.AddTypingRealmAuthentication(authority, audience);
        }

        public static void AddTypingRealmAuthentication(
            this IServiceCollection services,
            string authority,
            string audience)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hub"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IProfileTokenService, ProfileTokenService>();
            services.AddTransient<IProfileService, ProfileServiceAdapter>();
            services.AddTransient<IConnectHook, AuthorizeConnectHook>();
            services.AddTransient<IHttpClient, AuthenticatedHttpClient>();
        }
    }
}
