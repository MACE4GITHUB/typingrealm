using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TypingRealm.Authentication.OAuth;

namespace TypingRealm.Authentication.Api;

public static class RegistrationExtensions
{
    /// <summary>
    /// For pure Web API hosts.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTyrApiAuthentication(this IServiceCollection services)
    {
        var authInfoProvider = services.AddTyrCommonAuthentication();
        services.UseAspNetAuthentication(authInfoProvider);

        return services;
    }

    /// <summary>
    /// ASP.Net authentication & authorization, used by APIs and can be used
    /// by Services as well (for example, by SignalR hosts), but not by
    /// self-hosted services (e.g. with some custom TCP implementation).
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection UseAspNetAuthentication(
        this IServiceCollection services,
        IAuthenticationInformationProvider authenticationInformationProvider)
    {
        var profileAuthentication = authenticationInformationProvider.GetProfileAuthenticationInformation();
        var serviceAuthentication = authenticationInformationProvider.GetServiceAuthenticationInformation();
        var additionalProfileAuthentications = authenticationInformationProvider.GetAdditionalProfileAuthenticationInformations();

        if (profileAuthentication.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null
            || (serviceAuthentication != null && serviceAuthentication.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null)
            || additionalProfileAuthentications.Any(x => x.TokenValidationParameters.ValidAudiences?.FirstOrDefault() == null))
            throw new InvalidOperationException("Call UseSomeProvider method on AuthenticationInformationBuilder before calling this method, so that ValidAudiences parameter is set up.");

        var authenticationSchemes = new List<string>
            {
                profileAuthentication.SchemeName ?? throw new InvalidOperationException("Scheme name is empty.")
            };

        var authenticationBuilder = services
            .AddAuthentication(TyrAuthenticationSchemes.ProfileAuthenticationScheme) // Sets default authentication scheme.
            .AddJwtBearer(profileAuthentication.SchemeName, options => ConfigureOptions(options, profileAuthentication));

        foreach (var additionalAuthentication in additionalProfileAuthentications)
        {
            authenticationSchemes.Add(additionalAuthentication.SchemeName ?? throw new InvalidOperationException("Scheme name is empty."));
            authenticationBuilder.AddJwtBearer(additionalAuthentication.SchemeName, options => ConfigureOptions(options, additionalAuthentication));
        }

        if (serviceAuthentication != null)
        {
            authenticationSchemes.Add(serviceAuthentication.SchemeName ?? throw new InvalidOperationException("Scheme name is empty."));
            authenticationBuilder.AddJwtBearer(serviceAuthentication.SchemeName, options => ConfigureOptions(options, serviceAuthentication));
        }

        if (authenticationSchemes.Distinct().Count() != authenticationSchemes.Count)
            throw new InvalidOperationException("Duplicate authentication schemes found.");

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
        // TODO: Consider not using this flow at all as this can lead to logging access token in Web console.
        // Or configure it at least only when SignalR host is used.
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
}
