using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Communication;
using TypingRealm.Configuration;

namespace TypingRealm.Authentication.Api;

public sealed class RealtimeAuthenticationClient
{
    private readonly string _serviceName;
    private readonly IServiceClient _client;

    public RealtimeAuthenticationClient(
        IConfiguration configuration,
        IServiceClient client)
    {
        _serviceName = configuration.GetServiceId();
        _client = client;
    }

    public async ValueTask<string?> GetRealtimeAuthValueAsync(string token)
    {
        var result = await _client.PostAsync<object, string?>(
            _serviceName,
            "api/realtime-auth/validate",
            EndpointAuthentication.FromClientCredentials(
                new ClientCredentials("realtime-auth", "secret", new[] { "realtime-auth", "service" })),
            new { token = token }, default)
            .ConfigureAwait(false);

        return result;
    }
}

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
        services.AddTransient<RealtimeAuthenticationClient>();

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
        services.AddTransient<IProfileService, HttpContextProfileService>();

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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = async context =>
            {
                var queryToken = context.Request.Query["access_token"].FirstOrDefault();
                var headerToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ')[1];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/hub"))
                {
                    var token = queryToken ?? headerToken;
                    if (token == null)
                        return;

                    var realtimeAuthenticationClient = context.HttpContext.RequestServices.GetRequiredService<RealtimeAuthenticationClient>();

                    var actualTokenValue = await realtimeAuthenticationClient.GetRealtimeAuthValueAsync(token)
                        .ConfigureAwait(false);

                    if (string.IsNullOrEmpty(actualTokenValue))
                        return;

                    // Read the token out of the query string
                    context.Token = actualTokenValue;
                }
            },
            OnAuthenticationFailed = context =>
            {
                if (authenticationInformation.SuppressErrorOnDiscovery)
                    context.NoResult();

                return Task.CompletedTask;
            }
        };
    }
}
