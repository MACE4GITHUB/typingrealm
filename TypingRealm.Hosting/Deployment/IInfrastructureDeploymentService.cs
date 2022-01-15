using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Hosting.Deployment
{
    public interface IInfrastructureDeploymentService
    {
        ValueTask DeployInfrastructureAsync(CancellationToken cancellationToken);
    }
}
