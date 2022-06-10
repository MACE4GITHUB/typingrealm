using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Importing;
using TypingRealm.Library.InMemoryInfrastructure;
using TypingRealm.Library.Sentences;
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

    public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
    {
        // TODO: Implement and add here in memory IBookQuery, ISentenceQuery.

        return services.AddSingleton<InMemoryBookRepository>()
            .AddSingleton<InMemorySentenceRepository>()
            .AddTransient<IBookRepository>(p => p.GetRequiredService<InMemoryBookRepository>())
            .AddTransient<ISentenceRepository>(p => p.GetRequiredService<InMemorySentenceRepository>())
            .AddTransient<IBookQuery>(p => p.GetRequiredService<InMemoryBookRepository>())
            .AddTransient<SentenceQueryResolver>(p => _ => p.GetRequiredService<InMemorySentenceRepository>());
    }
}
