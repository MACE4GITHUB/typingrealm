using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Data.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterDataApi(this IServiceCollection services)
        {
            services.AddSingleton<ILocationRepository, InMemoryLocationRepository>();

            return services;
        }
    }
}
