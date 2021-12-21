using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Texts.Api.Client;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsApiClient(this IServiceCollection services)
    {
        return services.AddTransient<ITextsClient, TextsClient>();
    }
}
