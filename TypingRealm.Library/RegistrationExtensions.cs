using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Books;
using TypingRealm.Library.Importing;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryDomain(this IServiceCollection services)
    {
        services.AddTextProcessing();

        return services
            .AddTransient<IBookContentProcessor, BookContentProcessor>()
            .AddTransient<ISentenceFactory, SentenceFactory>()
            .AddTransient<IBookImporter, BookImporter>()
            .AddTransient<ArchiveBookService>();
    }
}
