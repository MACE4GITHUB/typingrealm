using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterProfilesApi(this IServiceCollection services)
        {
            services.AddSingleton<ICharacterRepository, InMemoryCharacterRepository>();
            services.AddTransient<ICharacterResourceQuery, InMemoryCharacterResourceQuery>();

            return services;
        }
    }
}
