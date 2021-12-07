using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Data.Infrastructure.DataAccess;

namespace TypingRealm.Data.Infrastructure
{
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
        private readonly DataContext _context;

        public InfrastructureDeploymentService(DataContext context)
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
}
