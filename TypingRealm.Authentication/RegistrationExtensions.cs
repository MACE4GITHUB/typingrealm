using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TypingRealm.Authentication.Adapters;
using TypingRealm.Communication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication
{
    public interface IAuthenticationInformationProvider
    {
        AuthenticationInformation GetProfileAuthenticationInformation();
        AuthenticationInformation GetServiceAuthenticationInformation();
    }

    public sealed class AuthenticationInformationProvider : IAuthenticationInformationProvider
    {
        private readonly AuthenticationInformation _profileAuthenticationInformation;
        private readonly AuthenticationInformation _serviceAuthenticationInformation;

        public AuthenticationInformationProvider(
            AuthenticationInformation profileAuthenticationInformation,
            AuthenticationInformation serviceAuthenticationInformation)
        {
            _profileAuthenticationInformation = profileAuthenticationInformation;
            _serviceAuthenticationInformation = serviceAuthenticationInformation;
        }

        public AuthenticationInformation GetProfileAuthenticationInformation()
        {
            return _profileAuthenticationInformation;
        }

        public AuthenticationInformation GetServiceAuthenticationInformation()
        {
            return _serviceAuthenticationInformation;
        }
    }

    public sealed class AuthenticationInformationBuilder
    {
        public AuthenticationInformation AuthenticationInformation { get; }
            = new AuthenticationInformation();

        public AuthenticationInformation Build() => AuthenticationInformation;
    }

    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTypingRealmAuthentication(
            this IServiceCollection services,
            AuthenticationInformation profileAuthentication,
            AuthenticationInformation? serviceAuthentication = null)
        {
            services.AddTransient<IAccessTokenProvider, AccessTokenProvider>();
            services.AddTransient<IServiceTokenService, ServiceTokenService>();
            services.AddProfileApiClients();

            services.AddSingleton<IAuthenticationInformationProvider>(new AuthenticationInformationProvider(
                profileAuthentication, serviceAuthentication ?? profileAuthentication));

            return services;
        }

        public static MessageTypeCacheBuilder AddTypingRealmAuthentication(
            this MessageTypeCacheBuilder builder,
            AuthenticationInformation profileAuthentication,
            AuthenticationInformation? serviceAuthentication = null)
        {
            builder.AddTyrAuthenticationMessages();
            builder.Services.AddTypingRealmAuthentication(profileAuthentication, serviceAuthentication);

            return builder;
        }

        public static IServiceCollection AddTyrApiAuthentication(this IServiceCollection services)
        {
            var profileAuthentication = BuildAuth0OrLocal();

            var serviceAuthentication = new AuthenticationInformationBuilder()
                .UseIdentityServerProvider()
                .Build();

            return services.AddTypingRealmAuthentication(profileAuthentication, serviceAuthentication)
                .UseAspNetAuthentication(profileAuthentication, serviceAuthentication);
        }

        public static MessageTypeCacheBuilder AddTyrWebServiceAuthentication(this MessageTypeCacheBuilder builder)
        {
            var profileAuthentication = BuildAuth0OrLocal();

            var serviceAuthentication = new AuthenticationInformationBuilder()
                .UseIdentityServerProvider()
                .Build();

            builder.AddTypingRealmAuthentication(profileAuthentication, serviceAuthentication)
                .Services
                .UseAspNetAuthentication(profileAuthentication, serviceAuthentication)
                .UseConnectedClientContextAuthentication()
                .UseCharacterAuthorizationOnConnect();

            return builder;
        }

        public static IServiceCollection UseAspNetAuthentication(
            this IServiceCollection services,
            AuthenticationInformation profileAuthentication,
            AuthenticationInformation? serviceAuthentication = null)
        {
            if (profileAuthentication.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null
                || (serviceAuthentication != null && serviceAuthentication.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null))
                throw new InvalidOperationException("Call UseSomeProvider method on AuthenticationInformationBuilder before calling this method, so that ValidAudiences parameter is set up.");

            var authenticationSchemes = new List<string>
            {
                TyrAuthenticationSchemes.ProfileAuthenticationScheme
            };

            var authenticationBuilder = services
                .AddAuthentication(TyrAuthenticationSchemes.ProfileAuthenticationScheme) // Sets default authentication scheme.
                .AddJwtBearer(TyrAuthenticationSchemes.ProfileAuthenticationScheme, options => ConfigureOptions(options, profileAuthentication));

            if (serviceAuthentication != null)
            {
                authenticationSchemes.Add(TyrAuthenticationSchemes.ServiceAuthenticationScheme);
                authenticationBuilder.AddJwtBearer(TyrAuthenticationSchemes.ServiceAuthenticationScheme, options => ConfigureOptions(options, serviceAuthentication));
            }

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(authenticationSchemes.ToArray())
                    .Build();
            });

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IProfileTokenService, HttpContextProfileTokenService>();

            return services;
        }

        private static void ConfigureOptions(JwtBearerOptions options, AuthenticationInformation authenticationInformation)
        {
            options.RequireHttpsMetadata = authenticationInformation.RequireHttpsMetadata;
            options.Audience = authenticationInformation.TokenValidationParameters.ValidAudiences.First();
            options.TokenValidationParameters = authenticationInformation.TokenValidationParameters;
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
        }

        // TODO: Move it to separate assembly, not related to AspNet.
        public static MessageTypeCacheBuilder AddTyrServiceWithoutAspNetAuthentication(this MessageTypeCacheBuilder builder)
        {
            var profileAuthentication = BuildAuth0OrLocal();

            var serviceAuthentication = new AuthenticationInformationBuilder()
                .UseIdentityServerProvider()
                .Build();

            builder.AddTypingRealmAuthentication(profileAuthentication, serviceAuthentication)
                .Services
                .UseConnectedClientContextAuthentication()
                .UseCharacterAuthorizationOnConnect();

            return builder;
        }

        public static IServiceCollection UseConnectedClientContextAuthentication(this IServiceCollection services)
        {
            services.AddTransient<IProfileTokenService, ProfileTokenService>();
            services.AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();
            services.Decorate<IConnectionInitializer, AuthenticateConnectionInitializer>();
            services.RegisterHandler<Authenticate, AuthenticateHandler>();

            // Scope is created per user connection, managed by Messaging project.
            services.AddScoped<IConnectedClientContext, ConnectedClientContext>();

            return services;
        }

        public static IServiceCollection UseCharacterAuthorizationOnConnect(this IServiceCollection services)
        {
            services.AddTransient<IProfileService, ProfileServiceAdapter>();
            services.AddTransient<IConnectHook, AuthorizeConnectHook>();

            return services;
        }

        public static MessageTypeCacheBuilder AddTyrAuthenticationMessages(this MessageTypeCacheBuilder builder)
        {
            return builder.AddMessageTypesFromAssembly(typeof(Authenticate).Assembly);
        }

        private static AuthenticationInformation BuildAuth0OrLocal()
        {
            if (DebugHelpers.UseLocalAuthentication)
            {
                return new AuthenticationInformationBuilder()
                    .UseLocalProvider()
                    .Build();
            }

            return new AuthenticationInformationBuilder()
                .UseAuth0Provider()
                .Build();
        }
    }
}
