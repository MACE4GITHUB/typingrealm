using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterDataApi(this IServiceCollection services)
        {
            services.AddSingleton<ILocationRepository, InMemoryLocationRepository>();

            // Typing.
            services.AddTransient<ITextGenerator, TextGenerator>();

            return services;
        }
    }
}
