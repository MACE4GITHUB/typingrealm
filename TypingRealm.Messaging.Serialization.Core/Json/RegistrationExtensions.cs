using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization.Json;

public static class RegistrationExtensions
{
    public static IServiceCollection UseJsonMessageSerializer(this IServiceCollection services)
    {
        services.AddTransient<IMessageSerializer, JsonMessageSerializer>();

        return services;
    }
}
