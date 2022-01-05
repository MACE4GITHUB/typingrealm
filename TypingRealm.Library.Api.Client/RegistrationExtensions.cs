using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Library.Api.Client;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryApiClients(this IServiceCollection services)
    {
        return services.AddTransient<ISentencesClient, SentencesClient>();
    }
}
