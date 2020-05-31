using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
            services.RegisterHandler<Join, JoinHandler>();
            services.RegisterHandler<MoveTo, MoveToHandler>();

            return services;
        }
    }
}
