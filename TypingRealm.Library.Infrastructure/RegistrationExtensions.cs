using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.DataAccess.Postgres;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Infrastructure.DataAccess;
using TypingRealm.Library.Infrastructure.DataAccess.Repositories;
using TypingRealm.Library.InMemoryInfrastructure;
using TypingRealm.Library.Sentences;
using TypingRealm.Library.Sentences.Queries;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Infrastructure;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLibraryDomain();

        if (DebugHelpers.UseInfrastructure)
        {
            services.AddLibraryDatabase(configuration);
        }
        else
        {
            // TODO: Fix local implementation: it doesn't work because some deps like IBookQuery are not registered.
            services.AddInMemoryInfrastructure();
        }

        return services;
    }

    private static IServiceCollection AddLibraryDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresWithEFMigrations<LibraryDbContext>(configuration);

        services.AddScoped<ISentenceRepository, SentenceRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBookQuery, BookQuery>();
        services.AddScoped<SentenceQueryResolver>(provider => language => new SentenceQuery(
            provider.GetRequiredService<LibraryDbContext>(),
            provider.GetRequiredService<ITextProcessor>(),
            language));

        return services;
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
