using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Authentication.Service;
using TypingRealm.Communication;
using TypingRealm.Data.Api.Client;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.RopeWar;
using TypingRealm.SignalR;
using TypingRealm.SignalR.Client;
using TypingRealm.World;

namespace TypingRealm.Hosting.ConsoleClient;

// TODO: Make sure everything is aligned with TypingRealm.Hosting.RegistrationExtensions.
public static class RegistrationExtensions
{
    /// <summary>
    /// This is a specific host that shouldn't register any server-side logic, only main framework for the front-end.
    /// </summary>
    public static MessageTypeCacheBuilder UseConsoleAppHost(this IServiceCollection services)
    {
        services.SetupCommonDependencies();

        var builder = services.AddSerializationCore();

        builder
            .AddTyrAuthenticationMessages()
            .AddWorldMessages()
            .AddRopeWarMessages();

        services
            .AddCommunication()
            .UseJsonMessageSerializer()
            .AddProtobufMessageSerializer()
            .RegisterClientMessaging() // Client-specific. TODO: use RegisterClientMessagingBase instead.
            .AddSignalRConnectionFactory()
            .AddProfileApiClients()
            .AddLocationApiClients()
            .RegisterClientConnectionFactoryFactory<SignalRClientConnectionFactoryFactory>()
            .AddAuth0ProfileTokenProvider();

        return builder;
    }

    private static IServiceCollection SetupCommonDependencies(this IServiceCollection services)
    {
        services.AddHttpClient();

        return services;
    }
}
