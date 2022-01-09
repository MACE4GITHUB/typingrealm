using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper;

public sealed record DeploymentData(
    IEnumerable<Service> Services,
    IEnumerable<Service> WebServices)
{
    public static readonly string ProjectName = Constants.ProjectName;
    private const string NetworkPostfix = "net";

    public IEnumerable<string> GetAllNetworks(Environment environment)
    {
        var serviceNetworks = Services
            .Where(service => !environment.OnlyMainNetwork)
            .Where(service => service.IsInEnvironment(environment))
            .Select(service => $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}")
            .ToList();

        var webServiceNetworks = WebServices
            .Where(service => !environment.OnlyMainNetwork)
            .Where(service => service.IsInEnvironment(environment))
            .Select(service => $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}")
            .ToList();

        var mainEnvironment = $"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}";

        return serviceNetworks
            .Concat(webServiceNetworks)
            .Append(mainEnvironment);
    }

    public static IEnumerable<string> GetNetworks(Service service, Environment environment)
    {
        yield return $"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}";
        if (environment.OnlyMainNetwork)
            yield break;

        yield return $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}";
    }

    public IEnumerable<ServiceInformation> GetServiceInformations(Environment environment)
    {
        var serviceInfos = Services
            .Where(service => service.IsInEnvironment(environment))
            .OrderBy(service => service.ServiceName)
            .SelectMany(service => GetDockerServices(service, environment))
            .ToList();

        var webServiceInfos = WebServices
            .Where(service => service.IsInEnvironment(environment))
            .OrderBy(service => service.ServiceName)
            .SelectMany(service => GetDockerServices(service, environment))
            .ToList();

        var result = serviceInfos.Concat(webServiceInfos);
        return result;
    }

    private static IEnumerable<ServiceInformation> GetDockerServices(Service service, Environment environment)
    {
        // TODO: Move to common place and reuse in GetNetworks.
        var dockerName = $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}";

        yield return new ServiceInformation(
            "${DOCKER_REGISTRY-}" + dockerName, dockerName,
            GetNetworks(service, environment),
            new BuildConfiguration(service.DockerBuildContext, service.DockerfilePath),
            "1g", "750m",
            GetEnvFiles(service, environment),
            GetServicePorts(service, environment),
            GetServiceVolumes(service, environment));

        if (environment.DeployInfrastructure)
        {
            if (service.DatabaseType == DatabaseType.Postgres)
            {
                yield return new ServiceInformation(
                    "postgres", $"{dockerName}-postgres",
                    GetNetworks(service, environment).Where(x => x.Contains(service.ServiceName)),
                    null,
                    "2g", "1.5g",
                    GetEnvFiles(service, environment),
                    environment.HideInfrastructurePorts
                        ? Enumerable.Empty<string>()
                        : new[] { GetInfrastructurePort(5432, environment, service) },
                    GetPostgresVolumes(service, environment));
            }

            if (service.CacheType == CacheType.Redis)
            {
                yield return new ServiceInformation(
                    "redis", $"{dockerName}-redis",
                    GetNetworks(service, environment).Where(x => x.Contains(service.ServiceName)),
                    null,
                    "2g", "1.5g",
                    GetEnvFiles(service, environment),
                    environment.HideInfrastructurePorts
                        ? Enumerable.Empty<string>()
                        : new[] { GetInfrastructurePort(6379, environment, service) },
                    GetRedisVolumes(service, environment));
            }
        }
    }

    private static IEnumerable<string> GetRedisVolumes(Service service, Environment environment)
    {
        yield return $"{Constants.InfrastructureDataFolder}/{environment.VolumeFolderName}/{service.ServiceName}/redis:/data";
    }

    private static IEnumerable<string> GetPostgresVolumes(Service service, Environment environment)
    {
        yield return $"{Constants.InfrastructureDataFolder}/{environment.VolumeFolderName}/{service.ServiceName}/postgres:/var/lib/postgresql/data";
    }

    private static IEnumerable<string> GetServiceVolumes(Service service, Environment environment)
    {
        _ = service;
        _ = environment;

        return Enumerable.Empty<string>();
    }

    private static IEnumerable<string> GetServicePorts(Service service, Environment environment)
    {
        if (environment.Value == "debug")
            return new[]
            {
                $"{service.Port}:80"
            };

        return Enumerable.Empty<string>();
    }

    public static string GetInfrastructurePort(int infrastructurePort, Environment environment, Service service)
    {
        var portPrefix = $"{environment.PortPrefix}{service.Index.ToString("D2")}";
        return $"{portPrefix}{infrastructurePort.ToString("D5").Substring(3, 2)}:{infrastructurePort}";
    }

    private static IEnumerable<string> GetEnvFiles(Service service, Environment environment)
    {
        yield return environment.EnvironmentFileName;

        if (service.ServiceName == Constants.WebUiServiceName)
            yield break;

        yield return $"{environment.EnvironmentFileName}.{service.ServiceName}";
    }
}
