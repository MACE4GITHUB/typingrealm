using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypingRealm.DeploymentHelper.EnvFiles;

public sealed class EnvFileGenerator
{
    public IEnumerable<EnvFile> GenerateEnvFiles(DeploymentData deploymentData, Environment environment)
    {
        if (!environment.GenerateEnvFiles)
            yield break;

        var envVars = new List<EnvVariable>();
        var serviceEnvVars = deploymentData.Services
            .Where(s => s.ServiceName != Constants.WebUiServiceName)
            .Select(x => new { Service = x, EnvVars = new List<EnvVariable>() })
            .ToDictionary(x => x.Service.ServiceName);

        if (environment.IsDevelopmentEnv)
            envVars.Add(new EnvVariable("ASPNETCORE_ENVIRONMENT", "Development"));

        foreach (var service in deploymentData.Services)
        {
            if (service.ServiceName == Constants.WebUiServiceName)
                continue;

            if (service.ServiceName == "identityserver")
            {
                if (environment.IsDebug)
                {
                    envVars.Add(new EnvVariable("SERVICE_AUTHORITY", $"http://host.docker.internal:{service.Port}/"));
                }
                else
                {
                    envVars.Add(new EnvVariable("SERVICE_AUTHORITY", $"http://{environment.EnvironmentPrefix}{DeploymentData.ProjectName}-{service.ServiceName}/"));
                }
                continue;
            }

            var serviceAddress = environment.IsDebug
                ? $"http://host.docker.internal:{service.Port}"
                : $"http://{environment.EnvironmentPrefix}{DeploymentData.ProjectName}-{service.ServiceName}";

            envVars.Add(new EnvVariable($"{service.ServiceName.ToUpperInvariant()}_URL", serviceAddress));

            if (service.CacheType == CacheType.Redis)
            {
                if (environment.IsDebug)
                {
                    var devRedisPort = DeploymentData.GetInfrastructurePort(6379, new Environment("dev"), service).Split(":")[0];
                    serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("ConnectionStrings__ServiceCacheConnection", $"host.docker.internal:{devRedisPort}"));
                }
                else
                {
                    serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("ConnectionStrings__ServiceCacheConnection", $"{environment.EnvironmentPrefix}{DeploymentData.ProjectName}-{service.ServiceName}-redis:6379"));
                }
            }

            if (service.DatabaseType == DatabaseType.Postgres)
            {
                serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("POSTGRES_PASSWORD", "admin"));
                if (environment.IsDebug)
                {
                    var server = $"host.docker.internal";
                    var devPostgresPort = DeploymentData.GetInfrastructurePort(5432, new Environment("dev"), service).Split(":")[0];
                    var connectionString = $"Server={server}; Port={devPostgresPort}; User Id=postgres; Password=admin; Database=db";
                    serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("ConnectionStrings__DataConnection", connectionString));
                }
                else
                {
                    var connectionString = $"Server={environment.EnvironmentPrefix}{DeploymentData.ProjectName}-{service.ServiceName}-postgres; Port=5432; User Id=postgres; Password=admin; Database=db";
                    serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("ConnectionStrings__DataConnection", connectionString));
                }
            }
        }

        var sb = new StringBuilder();
        foreach (var v in envVars.OrderBy(x => x.Name))
        {
            sb.AppendLine($"{v.Name}={v.Value}");
        }

        yield return new EnvFile($".env.{environment.Value}", sb.ToString());

        foreach (var serviceVars in serviceEnvVars.Values)
        {
            sb = new StringBuilder();
            foreach (var v in serviceVars.EnvVars.OrderBy(x => x.Name))
            {
                sb.AppendLine($"{v.Name}={v.Value}");
            }

            yield return new EnvFile($".env.{environment.Value}.{serviceVars.Service.ServiceName}", sb.ToString());
        }
    }
}
