using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Texts;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsDomain(this IServiceCollection services)
    {
        services.AddTransient<ITextGenerator, TextGenerator>();

        return services;
    }
}
