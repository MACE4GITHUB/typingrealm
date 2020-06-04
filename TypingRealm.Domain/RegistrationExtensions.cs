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
            services.AddTransient<IConnectionInitializer, ConnectionInitializer>();
            services.AddTransient<IPlayerFactory, PlayerFactory>();
            services.UseUpdateFactory<UpdateFactory>();

            services.RegisterHandler<MoveToLocation, MoveToLocationHandler>();

            return services;
        }
    }
}
