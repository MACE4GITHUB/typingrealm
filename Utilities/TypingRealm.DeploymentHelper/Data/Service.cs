using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Data;

public sealed record Service(
    int Index, /* Unique 0-99 index for port mapping mainly. */
    string ServiceName,
    DatabaseType DatabaseType,
    CacheType CacheType,
    string DockerBuildContext,
    string ProjectFolder,
    int Port,
    bool AddToReverseProxyInProduction)
{
    public IEnumerable<string>? Envs { get; set; }

    public string DockerfilePath => $"{(ProjectFolder == "?" ? "" : $"{ProjectFolder}/")}Dockerfile";

    public bool IsInEnvironment(Environment environment)
    {
        if (Envs == null || !Envs.Any())
        {
            return true;
        }

        return Envs.Contains(environment.Value);
    }
}
