using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper;

public sealed record Service(
    int Index, /* Unique 0-99 index for port mapping mainly. */
    string ServiceName,
    DatabaseType DatabaseType,
    CacheType CacheType,
    string DockerBuildContext,
    string DockerfilePath,
    int Port,
    bool AddToReverseProxyInProduction)
{
    public IEnumerable<string>? Envs { get; set; }

    public bool IsInEnvironment(Environment environment)
    {
        if (Envs == null || !Envs.Any())
        {
            return true;
        }

        return Envs.Contains(environment.Value);
    }
}
