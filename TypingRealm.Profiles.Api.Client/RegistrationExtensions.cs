using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Profiles.Api.Client
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddProfileApiClients(this IServiceCollection services)
        {
            return services.AddTransient<ICharactersClient, CharactersClient>();
        }
    }
}
