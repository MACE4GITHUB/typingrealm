using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Data.Infrastructure.DataAccess;
using TypingRealm.Hosting;

namespace TypingRealm.Data.Infrastructure
{
    public sealed class InfrastructureDeploymentService : IInfrastructureDeploymentService
    {
        private readonly DataContext _context;

        public InfrastructureDeploymentService(DataContext context)
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
}
