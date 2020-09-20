using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TypingRealm.Authentication.Adapters;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Authentication
{
    public static class LocalAuthentication
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler;
        private static readonly SigningCredentials _signingCredentials;

#pragma warning disable S3963, CA1810
        static LocalAuthentication()
        {
            SecurityKey = new SymmetricSecurityKey(new byte[32]);

            _tokenHandler = new JwtSecurityTokenHandler();
            _signingCredentials = new SigningCredentials(
                SecurityKey, SecurityAlgorithms.HmacSha256);
        }
#pragma warning restore S3963, CA1810

        internal static string Authority => "local-authority";
        internal static SecurityKey SecurityKey { get; }

        public static string GenerateJwtAccessToken(string subClaimValue)
        {
            var claims = new Claim[]
            {
                new Claim("sub", subClaimValue)
            };

            return _tokenHandler.WriteToken(new JwtSecurityToken(Authority, "https://api.typingrealm.com", claims, null, DateTime.UtcNow.AddMinutes(1), _signingCredentials));
        }
    }

    public static class AuthenticationExtensions
    {
        public static void AddLocalTypingRealmAuthentication(this IServiceCollection services)
        {
            // Slash at the end is mandatory - scopes issued by Auth0 have this authority.
            var authority = "https://local-authority";
            var audience = "https://api.typingrealm.com";

            services.AddTypingRealmAuthentication(authority, audience, isLocal: true);
        }

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
            string audience,
            bool isLocal = false)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                if (isLocal)
                {
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = LocalAuthentication.Authority
                    };
                    options.Configuration.SigningKeys.Add(LocalAuthentication.SecurityKey);
                }

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
