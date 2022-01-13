using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Books;
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
            services.AddTransient<IInfrastructureDeploymentService, NoInfrastructureService>();
            services.AddSingleton<ISentenceRepository, InMemorySentenceRepository>();
            services.AddSingleton<IBookRepository, InMemoryBookRepository>();
        }

        return services;
    }

    private static IServiceCollection AddLibraryDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Get data connection from common configuration (Hosting project or something).
        var dataConnectionString = configuration.GetConnectionString("DataConnection");
        services.AddDbContext<LibraryDbContext>(
            options => options
                .UseNpgsql(dataConnectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ISentenceRepository, SentenceRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<SentenceQueryResolver>(provider => language => new SentenceQuery(
            provider.GetRequiredService<LibraryDbContext>(), language));

        // TODO: Move out infrastructure deployment service to hosting project and reuse it.
        services.AddScoped<IInfrastructureDeploymentService, InfrastructureDeploymentService>();

        return services;
    }
}
