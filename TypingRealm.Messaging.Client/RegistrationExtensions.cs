using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Client.Handling;

namespace TypingRealm.Messaging.Client
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers messaging framework for client side: message dispatching
        /// and handling.
        /// </summary>
        public static IServiceCollection RegisterClientMessaging(this IServiceCollection services)
        {
            RegisterClientMessagingBase(services);
            services.AddSingleton<IMessageProcessor, MessageProcessor>();

            return services;
        }

        public static IServiceCollection RegisterClientMessagingForServer(this IServiceCollection services)
        {
            RegisterClientMessagingBase(services);
            services.AddTransient<IProfileTokenProvider, ServerProfileTokenProvider>();
            services.AddTransient<IMessageProcessorFactory, MessageProcessorFactory>();

            return services;
        }

        private static void RegisterClientMessagingBase(IServiceCollection services)
        {
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();
            services.AddTransient<IMessageHandlerFactory, MessageHandlerFactory>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
        }

        public static IServiceCollection RegisterHandler<TMessage, TMessageHandler>(this IServiceCollection services)
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>();

            return services;
        }
    }
}
