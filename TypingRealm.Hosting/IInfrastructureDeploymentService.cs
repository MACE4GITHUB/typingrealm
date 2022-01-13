using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Hosting
{
    public interface IInfrastructureDeploymentService
    {
        ValueTask DeployInfrastructureAsync(CancellationToken cancellationToken);
    }
}
