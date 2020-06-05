using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddDomainInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
            services.AddSingleton<ILocationStore, InMemoryLocationStore>();
            services.AddSingleton<IRoadStore, InMemoryRoadStore>();

            return services;
        }
    }
}
