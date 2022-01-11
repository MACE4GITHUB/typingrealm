using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Infrastructure.DataAccess;
using TypingRealm.Library.Infrastructure.DataAccess.Repositories;

namespace TypingRealm.Library.Infrastructure;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLibraryDomain();

        var dataConnectionString = configuration.GetConnectionString("DataConnection");
        services.AddDbContext<LibraryDbContext>(
            options => options
                .UseNpgsql(dataConnectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ISentenceRepository, SentenceRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<SentenceQueryResolver>(provider => language => new SentenceQuery(
            provider.GetRequiredService<LibraryDbContext>(), language));

        // TODO: Register NoInfrastructureService when no infrastructure.
        // TODO: Move out infrastructure deployment service to hosting project and reuse it.
        services.AddScoped<IInfrastructureDeploymentService, InfrastructureDeploymentService>();

        return services;
    }
}

// TODO: Generalize this, the same number of classes is registered in Data API.
public interface IInfrastructureDeploymentService
{
    ValueTask DeployInfrastructureAsync();
}

public sealed class NoInfrastructureService : IInfrastructureDeploymentService
{
    public ValueTask DeployInfrastructureAsync()
    {
        return default;
    }
}

public sealed class InfrastructureDeploymentService : IInfrastructureDeploymentService
{
    private readonly LibraryDbContext _context;

    public InfrastructureDeploymentService(LibraryDbContext context)
    {
        _context = context;
    }

    public async ValueTask DeployInfrastructureAsync()
    {
        // TODO: Try this until it succeeds (database can be down).
        await _context.Database.MigrateAsync()
            .ConfigureAwait(false);
    }
}
