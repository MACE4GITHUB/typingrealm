using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Data;

public sealed record ServiceProjects(
    string DomainPath,
    string ApiPath,
    string ApiClientPath,
    string CorePath,
    string InfrastructurePath)
{
    public IEnumerable<string> AllPaths => new[]
    {
        DomainPath,
        ApiPath,
        ApiClientPath,
        CorePath,
        InfrastructurePath
    };
}

public sealed record Service(
    int Index, /* Unique 0-99 index for port mapping mainly. */
    string RawServiceName,
    DatabaseType DatabaseType,
    CacheType CacheType,
    string DockerBuildContext,
    int Port,
    bool AddToReverseProxyInProduction)
{
    public IEnumerable<string>? Envs { get; set; }

    public string ServiceName => RawServiceName.ToLowerInvariant();
    public string BaseProjectFolder => $"{Constants.RawProjectName}.{RawServiceName}";

    public ServiceProjects ServiceProjects
    {
        get
        {
            var apiServicePath = $"{BaseProjectFolder}.Api";

            if (RawServiceName == Constants.AuthorityServiceName)
                apiServicePath = $"{BaseProjectFolder}.Host";

            return new ServiceProjects(
                BaseProjectFolder,
                apiServicePath,
                $"{apiServicePath}.Client",
                $"{BaseProjectFolder}.Core",
                $"{BaseProjectFolder}.Infrastructure");
        }
    }

    public string DockerfilePath
    {
        get
        {
            // HACKs on hacks :)
            if (RawServiceName == Constants.WebUiServiceName)
                return $"Dockerfile";

            return $"{ServiceProjects.ApiPath}/Dockerfile";
        }
    }

    public bool IsInEnvironment(Environment environment)
    {
        if (Envs == null || !Envs.Any())
        {
            return true;
        }

        return Envs.Contains(environment.Value);
    }
}
