﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers messaging framework for server side: message dispatching
        /// and handling, connection handling, sending updates.
        /// </summary>
        public static IServiceCollection RegisterMessaging(this IServiceCollection services)
        {
            // Connecting.
            // By default accept all connections without validation.
            services.AddTransient<IConnectionInitializer, AnonymousConnectionInitializer>();
            services.AddSingleton<IConnectedClientStore, ConnectedClientStore>();

            // Message dispatching and handling.
            services.AddTransient<IConnectionHandler, ConnectionHandler>();
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();
            services.AddTransient<IMessageHandlerFactory, MessageHandlerFactory>();

            // Handlers.
            services.RegisterHandler<Announce, AnnounceHandler>();
            services.RegisterHandler<Disconnect, DisconnectHandler>();

            // Updating.
            // By default announce update (for testing purposes).
            services.AddTransient<IUpdater, AnnouncingUpdater>();
            services.AddSingleton<IUpdateDetector, UpdateDetector>();

            return services;
        }

        public static IServiceCollection RegisterHandler<TMessage, TMessageHandler>(this IServiceCollection services)
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>();

            return services;
        }

        public static IServiceCollection UseUpdateFactory<TUpdateFactory>(this IServiceCollection services)
            where TUpdateFactory : class, IUpdateFactory
        {
            return services
                .AddTransient<IUpdater, Updater>()
                .AddTransient<IUpdateFactory, TUpdateFactory>();
        }
    }
}
