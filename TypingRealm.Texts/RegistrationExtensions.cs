using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Api.Client;

namespace TypingRealm.Texts;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsDomain(this IServiceCollection services)
    {
        services.AddLibraryApiClients();
        services.AddTransient<ITextGenerator, TextGenerator>();

        return services;
    }
}
