using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Combat;
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
            services.RegisterHandler<Attack, AttackHandler>();
            services.RegisterHandler<Surrender, SurrenderHandler>();

            services.RegisterHandler<EnterRoad, MovementHandler>();
            services.RegisterHandler<Move, MovementHandler>();
            services.RegisterHandler<TurnAround, MovementHandler>();

            return services;
        }
    }
}
