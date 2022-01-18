using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.DataAccess.Postgres;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Infrastructure.DataAccess;
using TypingRealm.Library.Infrastructure.DataAccess.Repositories;
using TypingRealm.Library.Sentences;
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
}
