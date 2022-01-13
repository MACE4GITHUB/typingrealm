using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Hosting;

namespace TypingRealm.DataAccess.Postgres;

public static class RegistrationExtensions
{
    public static IServiceCollection AddPostgresWithEFMigrations<TDbContext>(
        this IServiceCollection services, IConfiguration configuration)
        where TDbContext : DbContext
    {
        var dataConnectionString = configuration.GetConnectionString("DataConnection");

        services.AddDbContext<TDbContext>(options => options
            .UseNpgsql(dataConnectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IInfrastructureDeploymentService, EFDbContextMigrationDeploymentService<TDbContext>>();

        return services;
    }
}
