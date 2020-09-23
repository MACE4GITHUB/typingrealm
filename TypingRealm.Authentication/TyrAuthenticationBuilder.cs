using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace TypingRealm.Authentication
{
    public sealed class TyrAuthenticationBuilder
    {
        public TyrAuthenticationBuilder(IServiceCollection services)
        {
            Services = services;

            TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudiences = new[] { AuthenticationConfiguration.Audience },
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton(TokenValidationParameters);

            services.AddTransient<IHttpClient, AuthenticatedHttpClient>();
        }

        internal IServiceCollection Services { get; }
        internal TokenValidationParameters TokenValidationParameters { get; }
    }
}
