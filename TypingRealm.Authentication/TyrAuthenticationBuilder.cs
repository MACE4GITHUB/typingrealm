﻿using System;
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
                ValidAudiences = new[] { AuthenticationConfiguration.Audience },
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton(TokenValidationParameters);

            // One http client per connection.
            services.AddScoped<IHttpClient, AuthenticatedHttpClient>();
            services.AddProfileApiClients();
        }

        internal IServiceCollection Services { get; }
        internal TokenValidationParameters TokenValidationParameters { get; }
    }
}
