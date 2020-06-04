using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Messages;
using TypingRealm.Domain.Movement;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Domain
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<ILocationStore, InMemoryLocationStore>();
            services.AddTransient<IPlayerFactory, PlayerFactory>();

            services.RegisterHandler<MoveToLocation, MoveToLocationHandler>();

            services.UseUpdateFactory<UpdateFactory>();
            services.AddTransient<IConnectionInitializer, ConnectionInitializer>();

            return services;
        }
    }
}
