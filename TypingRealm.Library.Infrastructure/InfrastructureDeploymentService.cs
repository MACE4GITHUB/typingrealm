using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Hosting;
using TypingRealm.Library.Infrastructure.DataAccess;

namespace TypingRealm.Library.Infrastructure;

public sealed class InfrastructureDeploymentService : IInfrastructureDeploymentService
{
    private readonly LibraryDbContext _context;

    public InfrastructureDeploymentService(LibraryDbContext context)
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
