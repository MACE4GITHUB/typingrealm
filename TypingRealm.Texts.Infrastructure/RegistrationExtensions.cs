using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Texts.Infrastructure;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsApi(this IServiceCollection services)
    {
        services.AddTextsDomain();

        return services;
    }
}
