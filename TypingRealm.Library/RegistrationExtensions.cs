using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Importing;

namespace TypingRealm.Library;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryDomain(this IServiceCollection services)
    {
        return services.AddTransient<IBookImporter, BookImporter>();
    }
}
