using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypingRealm.DeploymentHelper
{
    public sealed record BuildConfiguration(
        string Context, string Dockerfile);

    public enum DatabaseType
    {
        None,
        Postgres
    }

    public enum CacheType
    {
        None,
        Redis
    }

    public sealed record Service(
        int Index, /* Unique 0-99 index for port mapping mainly. */
        string ServiceName,
        DatabaseType DatabaseType,
        CacheType CacheType,
        string DockerBuildContext,
        string DockerfilePath,
        int Port)
    {
        public IEnumerable<string>? Envs { get; set; }
    }

    public sealed record DeploymentData(
        IEnumerable<Service> Services)
    {
        public static readonly string ProjectName = "typingrealm";
        private const string NetworkPostfix = "net";

        public IEnumerable<string> GetAllNetworks(Environment environment)
        {
            // TODO: For prod/host env add external dev/local networks.

            return Services
                .Select(service => $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}")
                .Append($"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}")
                .ToList();
        }

        public IEnumerable<string> GetNetworks(Service service, Environment environment)
        {
            return new[]
            {
                $"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}",
                $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}"
            };
        }

        public IEnumerable<ServiceInformation> GetServiceInformations(Environment environment)
        {
            var serviceInfos = Services
                .Where(service => service.Envs == null || service.Envs.Contains(environment.Value))
                .OrderBy(service => service.ServiceName)
                .SelectMany((service, index) => GetDockerServices(service, environment, index))
                .ToList();

            // TODO: Add Caddy for some envs like "prod/host" and "local".

            return serviceInfos;
        }

        private IEnumerable<ServiceInformation> GetDockerServices(Service service, Environment environment, int index)
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
                            : new[] { GetInfrastructurePort(5432, environment, index) },
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
                            : new[] { GetInfrastructurePort(6379, environment, index) },
                        GetRedisVolumes(service, environment));
                }
            }
        }

        private IEnumerable<string> GetRedisVolumes(Service service, Environment environment)
        {
            yield return $"./infrastructure-data/{environment.VolumeFolderName}/{service.ServiceName}/redis:/data";
        }

        private IEnumerable<string> GetPostgresVolumes(Service service, Environment environment)
        {
            yield return $"./infrastructure-data/{environment.VolumeFolderName}/{service.ServiceName}/postgres:/var/lib/postgresql/data";
        }

        private IEnumerable<string> GetServiceVolumes(Service service, Environment environment)
        {
            _ = service;
            _ = environment;

            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetServicePorts(Service service, Environment environment)
        {
            if (environment.Value == "debug")
                return new[]
                {
                    $"{service.Port}:80"
                };

            return Enumerable.Empty<string>();
        }

        private string GetInfrastructurePort(int infrastructurePort, Environment environment, int index)
        {
            var portPrefix = $"{environment.PortPrefix}{index.ToString("D2")}";
            return $"{portPrefix}{infrastructurePort.ToString("D5").Substring(3, 2)}:{infrastructurePort}";
        }

        private IEnumerable<string> GetEnvFiles(Service service, Environment environment)
        {
            return new[]
            {
                environment.EnvironmentFileName,
                $"{environment.EnvironmentFileName}.{service.ServiceName}"
            };
        }
    }

    public sealed record ServiceInformation(
        string ImageName, string ContainerName,
        IEnumerable<string> Networks,
        BuildConfiguration? BuildConfiguration,
        string MemLimit, string MemReservation,
        IEnumerable<string> EnvFiles,
        IEnumerable<string> Ports,
        IEnumerable<string> Volumes);

    public sealed class Environment
    {
        public Environment(string value)
        {
            Value = value;
            PortPrefix = "999";

            if (value == "prod" || value == "strict-prod")
            {
                EnvironmentPrefix = string.Empty;
                EnvironmentFileName = ".env.prod";
                VolumeFolderName = "prod";

                if (value == "prod")
                    PortPrefix = "1";
                if (value == "strict-prod")
                    PortPrefix = "2";

                return;
            }

            if (value == "dev" || value == "local" || value == "debug")
            {
                EnvironmentPrefix = $"{value}-";
                EnvironmentFileName = $".env.{value}";
                VolumeFolderName = value;

                if (value == "dev")
                    PortPrefix = "3";
                if (value == "local")
                    PortPrefix = "4";
                if (value == "debug")
                    PortPrefix = "5";

                return;
            }

            throw new NotSupportedException($"Environment value {value} is not supported.");
        }

        public string EnvironmentPrefix { get; }
        public string Value { get; }
        public string EnvironmentFileName { get; }

        // Should have single character.
        public string PortPrefix { get; }
        public string VolumeFolderName { get; }
        public bool HideInfrastructurePorts => Value == "strict-prod" || Value == "local";
        public bool RestartUnlessStopped => Value != "debug";
        public bool DeployInfrastructure => Value != "debug";
        public bool IsDevelopmentEnv => Value != "prod" && Value != "strict-prod";
    }

    // TODO: Handle "Caddy" service separately (hardcode?).

    public sealed record EnvFile(string Name, string Data);
    public sealed class EnvFileGenerator
    {
        public IEnumerable<EnvFile> GenerateEnvFiles(DeploymentData deploymentData, Environment environment)
        {
            var envVars = new List<EnvVariable>();
            var serviceEnvVars = deploymentData.Services
                .Select(x => new { Service = x, EnvVars = new List<EnvVariable>() })
                .ToDictionary(x => x.Service.ServiceName);

            if (environment.IsDevelopmentEnv)
            {
                envVars.Add(new EnvVariable("ASPNETCORE_ENVIRONMENT", "Development"));

                foreach (var service in deploymentData.Services)
                {
                    if (service.ServiceName == "identityserver")
                    {
                        envVars.Add(new EnvVariable("SERVICE_AUTHORITY", $"http://{environment.EnvironmentPrefix}-{DeploymentData.ProjectName}-{service.ServiceName}/"));
                        continue;
                    }

                    envVars.Add(new EnvVariable($"{service.ServiceName.ToUpperInvariant()}_URL", "http://{environment.EnvironmentPrefix}-{DeploymentData.ProjectName}"));

                    if (service.CacheType == CacheType.Redis)
                    {
                        serviceEnvVars[service.ServiceName].EnvVars.Add(new EnvVariable("ConnectionStrings__ServiceCacheConnection", $"{environment.EnvironmentPrefix}-{DeploymentData.ProjectName}-{service.ServiceName}-redis:6379"));
                    }

                    if (service.DatabaseType == DatabaseType.Postgres)
                    {
                        var connectionString = $"{environment.EnvironmentPrefix}-{DeploymentData.ProjectName}-{service.ServiceName}-postgres; Port=5432; User Id=postgres; Password=admin; Database=db";
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

    public sealed record EnvVariable(string Name, string Value)
    {
        public override string ToString() => $"{Name}={Value}";
    }

    public sealed class DockerComposeGenerator
    {
        private const string Version = "3.4";

        public string Generate(DeploymentData deploymentData, Environment environment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("version: '3.4'");
            sb.AppendLine();

            // Declare networks.
            sb.AppendLine("networks:");
            foreach (var network in deploymentData.GetAllNetworks(environment))
            {
                sb.AppendLine($"  {network}:");
            }
            sb.AppendLine();

            // Declare services.
            sb.AppendLine("services:");

            foreach (var service in deploymentData.GetServiceInformations(environment))
            {
                sb.AppendLine($"  {service.ContainerName}:");
                sb.AppendLine($"    image: {service.ImageName}");
                sb.AppendLine($"    container_name: {service.ContainerName}");
                sb.AppendLine($"    networks:");

                foreach (var network in service.Networks)
                {
                    sb.AppendLine($"      - {network}");
                }

                if (service.Ports.Any())
                {
                    sb.AppendLine($"    ports:");

                    foreach (var port in service.Ports)
                    {
                        sb.AppendLine($"      - {port}");
                    }
                }

                if (service.Volumes.Any())
                {
                    sb.AppendLine($"    volumes:");

                    foreach (var volume in service.Volumes)
                    {
                        sb.AppendLine($"      - {volume}");
                    }
                }

                if (service.BuildConfiguration != null)
                {
                    sb.AppendLine($"    build:");
                    sb.AppendLine($"      context: {service.BuildConfiguration.Context}");
                    sb.AppendLine($"      dockerfile: {service.BuildConfiguration.Dockerfile}");
                }

                if (environment.RestartUnlessStopped)
                    sb.AppendLine($"    restart: unless-stopped");

                sb.AppendLine($"    mem_limit: {service.MemLimit}");
                sb.AppendLine($"    mem_reservation: {service.MemReservation}");

                if (service.EnvFiles.Any())
                {
                    sb.AppendLine("    env_file:");
                    foreach (var envFile in service.EnvFiles)
                    {
                        sb.AppendLine($"      - {envFile}");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
