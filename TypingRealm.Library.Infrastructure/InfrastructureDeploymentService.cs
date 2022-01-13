using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Infrastructure.DataAccess;

namespace TypingRealm.Library.Infrastructure;

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
