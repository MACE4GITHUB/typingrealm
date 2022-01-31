using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.Service.Handlers;
using TypingRealm.Authentication.Service.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication.Service;

public static class RegistrationExtensions
{
    /// <summary>
    /// For self-hosted services without ASP.Net, like TCP real-time hosts
    /// based on messaging framework.
    /// </summary>
    public static MessageTypeCacheBuilder AddTyrServiceWithoutAspNetAuthentication(this MessageTypeCacheBuilder builder)
    {
        builder.Services.AddTyrCommonAuthentication();
        builder.AddMessagingServiceAuthentication();

        return builder;
    }

    /// <summary>
    /// This method is used by Console App clients only.
    /// </summary>
    public static MessageTypeCacheBuilder AddTyrAuthenticationMessages(this MessageTypeCacheBuilder builder)
    {
        return builder.AddMessageTypesFromAssembly(typeof(Authenticate).Assembly);
    }

    /// <summary>
    /// Use this method in domains that need to authenticate properly.
    /// </summary>
    // TODO: FORCE this method whenever we are not using character authentication.
    // We can't allow people to use random ClientIds.
    // Rethink the whole architecture here, possibly refactor.
    public static IServiceCollection AddAuthorizeConnectHook(this IServiceCollection services)
    {
        // Authorize Character: first message after authentication should be
        // Connect message with valid Character belonging to current Profile.
        services.AddTransient<IConnectHook, AuthorizeConnectHook>();

        return services;
    }

    /// <summary>
    /// Use this method in domains that need to check that Character belongs to the actual Profile.
    /// </summary>
    public static IServiceCollection AddCharacterAuthentication(this IServiceCollection services)
    {
        // Authorize Character: first message after authentication should be
        // Connect message with valid Character belonging to current Profile.
        services.AddTransient<IConnectHook, AuthorizeCharacterConnectHook>();

        return services;
    }

    internal static MessageTypeCacheBuilder AddMessagingServiceAuthentication(this MessageTypeCacheBuilder builder)
    {
        var services = builder.Services;

        builder.AddTyrAuthenticationMessagesAndHandlers();

        // Adding ConnectedClient (realtime) token service for realtime servers
        // that takes data from IConnectedClientContext.
        // Scope is created per user connection, managed by Messaging project.
        // It's very important to register it as scoped here.
        services.AddScoped<IConnectedClientContext, ConnectedClientContext>();
        services.AddTransient<IProfileTokenService, ConnectedClientProfileTokenService>();

        // Wait for Authenticate message with valid token as the first message.
        services.Decorate<IConnectionInitializer, AuthenticateConnectionInitializer>();

        // To query Characters API.
        services.AddProfileApiClients();

        return builder;
    }

    private static MessageTypeCacheBuilder AddTyrAuthenticationMessagesAndHandlers(this MessageTypeCacheBuilder builder)
    {
        builder.AddMessageTypesFromAssembly(typeof(Authenticate).Assembly);
        builder.Services.RegisterHandler<Authenticate, AuthenticateHandler>();

        return builder;
    }
}
