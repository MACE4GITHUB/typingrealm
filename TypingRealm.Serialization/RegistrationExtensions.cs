using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Serialization;

public static class RegistrationExtensions
{
    public static IServiceCollection AddSerialization(this IServiceCollection services)
    {
        services.AddTransient<ISerializer, Serializer>();

        return services;
    }
}
