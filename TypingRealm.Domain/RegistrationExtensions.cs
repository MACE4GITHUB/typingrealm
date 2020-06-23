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
            services.UseUpdateFactory<UpdateFactory>();

            services.RegisterHandler<MoveToLocation, MoveToLocationHandler>();

            services.RegisterHandler<EnterRoad, RoadMovementHandler>();
            services.RegisterHandler<Move, RoadMovementHandler>();
            services.RegisterHandler<TurnAround, RoadMovementHandler>();

            services.RegisterHandler<TeleportPlayerToLocation, TeleportPlayerToLocationHandler>();

            return services;
        }
    }
}
