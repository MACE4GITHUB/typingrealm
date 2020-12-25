using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TypingRealm.Communication;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication
{
    public sealed class TyrAuthenticationBuilder
    {
        public TyrAuthenticationBuilder(IServiceCollection services)
        {
            Services = services;

            TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudiences = new[] { Auth0AuthenticationConfiguration.Audience },
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton(TokenValidationParameters);
            services.AddTransient<IAccessTokenProvider, AccessTokenProvider>();
            services.AddTransient<IServiceTokenService, ServiceTokenService>();

            services.AddProfileApiClients();

            services.AddSingleton(AuthenticationInformation);
        }

        internal IServiceCollection Services { get; }
        internal TokenValidationParameters TokenValidationParameters { get; }
        internal AuthenticationInformation AuthenticationInformation { get; }
            = new AuthenticationInformation();
    }
}
