using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TypingRealm.Authentication.Adapters;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Authentication
{
    public static class TyrAuthenticationBuilderExtensions
    {
        public static TyrAuthenticationBuilder UseAuth0Provider(this TyrAuthenticationBuilder builder)
        {
            var parameters = builder.TokenValidationParameters;

            builder.AuthenticationInformation.AuthorizationEndpoint = Auth0AuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = Auth0AuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = "QEYu5cQ7etxQyUARibsLFJwh5HBNOcjv";
            builder.AuthenticationInformation.ServiceClientSecret = "uFj6pnafZFYZbabfub_5fdSZC2aQ-Sgw4VP7jsEWgWZQjv4OVMvkFqk25BBpC1Me";

            parameters.ValidAudiences = new[] { Auth0AuthenticationConfiguration.Audience };
            parameters.ValidIssuer = Auth0AuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{parameters.ValidIssuer}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

                return openIdConfig.SigningKeys;
            };

            return builder;
        }

        public static TyrAuthenticationBuilder UseIdentityServerProvider(this TyrAuthenticationBuilder builder)
        {
            var parameters = builder.TokenValidationParameters;

            builder.AuthenticationInformation.AuthorizationEndpoint = IdentityServerAuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = IdentityServerAuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = "service";
            builder.AuthenticationInformation.ServiceClientSecret = "secret";

            parameters.ValidAudiences = new[] { IdentityServerAuthenticationConfiguration.Audience };
            parameters.ValidIssuer = IdentityServerAuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{parameters.ValidIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

                return openIdConfig.SigningKeys;
            };

            return builder;
        }

        public static TyrAuthenticationBuilder UseLocalProvider(this TyrAuthenticationBuilder builder)
        {
            var parameters = builder.TokenValidationParameters;

            builder.AuthenticationInformation.AuthorizationEndpoint = LocalAuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = LocalAuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = "local-service";
            builder.AuthenticationInformation.ServiceClientSecret = "local-secret";

            parameters.ValidAudiences = new[] { LocalAuthenticationConfiguration.Audience };
            parameters.ValidIssuer = LocalAuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKeyResolver = null;
            parameters.IssuerSigningKey = LocalAuthentication.SecurityKey;

            return builder;
        }

        public static TyrAuthenticationBuilder UseConnectedClientContextAuthentication(this TyrAuthenticationBuilder builder)
        {
            var services = builder.Services;

            services.AddTransient<IProfileTokenService, ProfileTokenService>();
            services.AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();
            services.Decorate<IConnectionInitializer, AuthenticateConnectionInitializer>();
            services.RegisterHandler<Authenticate, AuthenticateHandler>();

            // Scope is created per user connection, managed by Messaging project.
            services.AddScoped<IConnectedClientContext, ConnectedClientContext>();

            return builder;
        }

        public static TyrAuthenticationBuilder UseAspNetAuthentication(this TyrAuthenticationBuilder builder)
        {
            var services = builder.Services;

            if (builder.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null)
                throw new InvalidOperationException("Call UseSomeProvider method before calling this method, so that ValidAudiences parameter is set up.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Audience = builder.TokenValidationParameters.ValidAudiences.First();
                options.TokenValidationParameters = builder.TokenValidationParameters;
                options.Authority = options.TokenValidationParameters.ValidIssuer;

                // HACK: Avoid getting .well-known when using local tokens.
                if (options.Authority == LocalAuthentication.Issuer)
                {
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = options.Authority
                    };
                    options.Configuration.SigningKeys.Add(options.TokenValidationParameters.IssuerSigningKey);
                }

                // For SignalR hubs.
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
            services.AddTransient<IProfileTokenService, HttpContextProfileTokenService>();

            return builder;
        }

        public static TyrAuthenticationBuilder UseCharacterAuthorizationOnConnect(this TyrAuthenticationBuilder builder)
        {
            var services = builder.Services;

            services.AddTransient<IProfileService, ProfileServiceAdapter>();
            services.AddTransient<IConnectHook, AuthorizeConnectHook>();

            return builder;
        }
    }
}
