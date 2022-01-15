using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Hosting.Deployment;

namespace TypingRealm.DataAccess.Postgres;

public sealed class EFDbContextMigrationDeploymentService<TDbContext> : IInfrastructureDeploymentService
    where TDbContext : DbContext
{
    private readonly TDbContext _context;

    public EFDbContextMigrationDeploymentService(TDbContext context)
    {
        _context = context;
    }

    public async ValueTask DeployInfrastructureAsync(CancellationToken cancellationToken)
    {
        // TODO: Try this until it succeeds (database can be down).
        await _context.Database.MigrateAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
