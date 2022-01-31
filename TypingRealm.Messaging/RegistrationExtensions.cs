using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging;

public static class RegistrationExtensions
{
    /// <summary>
    /// Registers messaging framework for server side: message dispatching
    /// and handling, connection handling, sending updates.
    /// </summary>
    public static IServiceCollection RegisterMessaging(this IServiceCollection services)
    {
        // Connection handling. Entry point.
        services.AddTransient<IConnectionHandler, ConnectionHandler>();
        services.AddTransient<IScopedConnectionHandler, ScopedConnectionHandler>();

        // Shared between all connections.
        services.AddSingleton<IConnectedClientStore, ConnectedClientStore>();
        services.AddSingleton<IUpdateDetector, UpdateDetector>();

        // Connecting.
        services.AddTransient<IConnectHook, EmptyConnectHook>();
        services.AddTransient<IConnectionInitializer, ConnectInitializer>();

        // Message dispatching and handling.
        services.AddTransient<IMessageDispatcher, MessageDispatcher>();
        services.AddTransient<IMessageHandlerFactory, MessageHandlerFactory>();

        services.AddSingleton<QueryHandlerMethodCache>();
        services.AddTransient<IQueryDispatcher, QueryDispatcher>();
        services.AddTransient<IQueryHandlerFactory, QueryHandlerFactory>();

        // Handlers.
        services.RegisterHandler<Announce, AnnounceHandler>();
        services.RegisterHandler<Disconnect, DisconnectHandler>();

        // Updating.
        // By default no updater.
        // For testing purposes, use AnnouncingUpdater.
        services.AddTransient<IUpdater, NoUpdater>();

        return services;
    }

    public static IServiceCollection RegisterHandler<TMessage, TMessageHandler>(this IServiceCollection services)
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>();

        return services;
    }

    public static IServiceCollection RegisterQueryHandler<TQuery, TQueryHandler, TResponse>(this IServiceCollection services)
        where TQueryHandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddTransient<IQueryHandler<TQuery, TResponse>, TQueryHandler>();

        return services;
    }

    public static IServiceCollection UseConnectionInitializer<TConnectionInitializer>(this IServiceCollection services)
        where TConnectionInitializer : class, IConnectionInitializer
    {
        return services.AddTransient<IConnectionInitializer, TConnectionInitializer>();
    }

    public static IServiceCollection UseUpdateFactory<TUpdateFactory>(this IServiceCollection services)
        where TUpdateFactory : class, IUpdateFactory
    {
        return services
            .AddTransient<IUpdater, Updater>()
            .AddTransient<IUpdateFactory, TUpdateFactory>();
    }
}
