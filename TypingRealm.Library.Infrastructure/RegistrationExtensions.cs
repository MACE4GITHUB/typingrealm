using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.DataAccess.Postgres;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Infrastructure.DataAccess;
using TypingRealm.Library.Infrastructure.DataAccess.Repositories;
using TypingRealm.Library.Infrastructure.InMemory;
using TypingRealm.Library.Sentences;

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
            services.AddSingleton<ISentenceRepository, InMemorySentenceRepository>();
            services.AddSingleton<IBookRepository, InMemoryBookRepository>();

            // TODO: Implement and add here in memory IBookQuery, ISentenceQuery.
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
            provider.GetRequiredService<LibraryDbContext>(), language));

        return services;
    }
}
