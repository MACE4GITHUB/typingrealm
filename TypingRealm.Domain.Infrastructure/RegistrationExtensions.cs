using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Domain.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddDomainInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();

            return services;
        }
    }
}
