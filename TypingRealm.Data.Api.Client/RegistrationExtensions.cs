using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Data.Api.Client;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLocationApiClients(this IServiceCollection services)
    {
        return services.AddTransient<ILocationsClient, LocationsClient>();
    }
}
