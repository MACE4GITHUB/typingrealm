using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypingRealm.DeploymentHelper
{
    public sealed class CaddyProfile
    {
        public CaddyProfile(string value)
        {
            if (value != "prod" && value != "host" && value != "local")
                throw new ArgumentException("CaddyProfile value is not correct.", nameof(value));

            Value = value;
        }

        public string Value { get; }
    }

    public sealed class CaddyfileGenerator
    {
        public string GenerateCaddyfile(DeploymentData data, CaddyProfile profile)
        {
            var sb = new StringBuilder();
            if (profile.Value != "local")
            {
                sb.AppendLine("{");
                sb.AppendLine("    email typingrealm@gmail.com");
                sb.AppendLine("}");
                sb.AppendLine();

                sb.AppendLine("typingrealm.com {");
                sb.AppendLine("    reverse_proxy typingrealm-web-ui:80");
                sb.AppendLine("}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("localhost {");
                sb.AppendLine("    reverse_proxy host.docker.internal:4200");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (profile.Value == "local")
            {
                sb.AppendLine("api.localhost {");
            }
            else
            {
                sb.AppendLine("api.typingrealm.com {");
            }

            foreach (var serviceName in data.Services
                .Where(s => s.ServiceName != "web-ui" && (s.AddToReverseProxyInProduction || profile.Value != "prod"))
                .Select(s => s.ServiceName)
                .OrderBy(name => name))
            {
                sb.AppendLine($"    handle_path /{serviceName}/* {{");
                sb.AppendLine($"        reverse_proxy {(profile.Value == "local" ? "local-" : "")}typingrealm-{serviceName}:80");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("    respond 404");
            sb.AppendLine("}");

            if (profile.Value == "host")
            {
                sb.AppendLine();
                sb.AppendLine("dev.typingrealm.com {");
                sb.AppendLine("    reverse_proxy dev-typingrealm-web-ui:80");
                sb.AppendLine("}");
                sb.AppendLine();

                sb.AppendLine("dev.api.typingrealm.com {");

                foreach (var serviceName in data.Services
                    .Where(s => s.ServiceName != "web-ui" && (s.AddToReverseProxyInProduction || profile.Value != "prod"))
                    .Select(s => s.ServiceName)
                    .OrderBy(name => name))
                {
                    sb.AppendLine($"    handle_path /{serviceName}/* {{");
                    sb.AppendLine($"        reverse_proxy dev-typingrealm-{serviceName}:80");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                sb.AppendLine("    respond 404");
                sb.AppendLine("}");
                sb.AppendLine();

                sb.AppendLine("localhost {");
                sb.AppendLine("    reverse_proxy host.docker.internal:4200");
                sb.AppendLine("}");
                sb.AppendLine();

                sb.AppendLine("api.localhost {");

                foreach (var serviceName in data.Services
                    .Where(s => s.ServiceName != "web-ui" && (s.AddToReverseProxyInProduction || profile.Value != "prod"))
                    .Select(s => s.ServiceName)
                    .OrderBy(name => name))
                {
                    sb.AppendLine($"    handle_path /{serviceName}/* {{");
                    sb.AppendLine($"        reverse_proxy local-typingrealm-{serviceName}:80");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                sb.AppendLine("    respond 404");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }

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
        int Port,
        bool AddToReverseProxyInProduction)
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
                .Where(service => service.Envs == null || service.Envs.Contains(environment.Value))
                .Where(service => environment.Value != "debug")
                .Select(service => $"{environment.EnvironmentPrefix}{ProjectName}-{service.ServiceName}-{NetworkPostfix}")
                .Append($"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}")
                .ToList();
        }

        public IEnumerable<string> GetNetworks(Service service, Environment environment)
        {
            if (environment.Value == "debug")
                return new[] { $"{environment.EnvironmentPrefix}{ProjectName}-{NetworkPostfix}" };

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
                .SelectMany(service => GetDockerServices(service, environment))
                .ToList();

            // TODO: Add Caddy for some envs like "prod/host" and "local".

            return serviceInfos;
        }

        private IEnumerable<ServiceInformation> GetDockerServices(Service service, Environment environment)
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

        public static string GetInfrastructurePort(int infrastructurePort, Environment environment, Service service)
        {
            var portPrefix = $"{environment.PortPrefix}{service.Index.ToString("D2")}";
            return $"{portPrefix}{infrastructurePort.ToString("D5").Substring(3, 2)}:{infrastructurePort}";
        }

        private IEnumerable<string> GetEnvFiles(Service service, Environment environment)
        {
            if (service.ServiceName == "web-ui")
                return new[] { environment.EnvironmentFileName };

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
                EnvironmentFileName = "deployment/.env.prod";
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
                EnvironmentFileName = $"deployment/.env.{value}";
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
        public bool GenerateEnvFiles => Value != "strict-prod";
        public bool IsDebug => Value == "debug";
    }

    // TODO: Handle "Caddy" service separately (hardcode?).

    public sealed record EnvFile(string Name, string Data);
    public sealed class EnvFileGenerator
    {
        public IEnumerable<EnvFile> GenerateEnvFiles(DeploymentData deploymentData, Environment environment)
        {
            if (!environment.GenerateEnvFiles)
                yield break;

            var envVars = new List<EnvVariable>();
            var serviceEnvVars = deploymentData.Services
                .Where(s => s.ServiceName != "web-ui")
                .Select(x => new { Service = x, EnvVars = new List<EnvVariable>() })
                .ToDictionary(x => x.Service.ServiceName);

            if (environment.IsDevelopmentEnv)
                envVars.Add(new EnvVariable("ASPNETCORE_ENVIRONMENT", "Development"));

            foreach (var service in deploymentData.Services)
            {
                if (service.ServiceName == "web-ui")
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

            if (environment.Value == "prod")
            {
                // HOST production, needs access to external networks:
                sb.AppendLine("  local-tyr_local-typingrealm-net:");
                sb.AppendLine("    external: true");
                sb.AppendLine("  dev-tyr_dev-typingrealm-net:");
                sb.AppendLine("    external: true");
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

            if (environment.Value == "local")
            {
                sb.AppendLine(@"
  local-typingrealm-caddy:
    image: caddy
    container_name: local-typingrealm-caddy
    networks:
      - local-typingrealm-net
    ports:
      - 80:80
      - 443:443
    restart: unless-stopped
    volumes:
      - ./reverse-proxy/Caddyfile.local:/etc/caddy/Caddyfile
      - ./infrastructure-data/local/caddy_data:/data
    mem_limit: 1g
    mem_reservation: 750m
    env_file:
      - deployment/.env.local");
            }
            else if (environment.Value == "strict-prod")
            {
                sb.AppendLine(@"
  typingrealm-caddy:
    image: caddy
    container_name: typingrealm-caddy
    networks:
      - typingrealm-net
    ports:
      - 80:80
      - 443:443
    volumes:
      - ./reverse-proxy/Caddyfile.prod:/etc/caddy/Caddyfile
      - ./infrastructure-data/prod/caddy_data:/data
    restart: unless-stopped
    mem_limit: 1g
    mem_reservation: 750m
    env_file:
      - deployment/.env.prod");
            }
            else if (environment.Value == "prod")
            {
                sb.AppendLine(@"
  typingrealm-caddy:
    image: caddy
    container_name: typingrealm-caddy
    networks:
      - typingrealm-net
      - local-tyr_local-typingrealm-net
      - dev-tyr_dev-typingrealm-net
    ports:
      - 80:80
      - 443:443
    volumes:
      - ./reverse-proxy/Caddyfile.host:/etc/caddy/Caddyfile
      - ./infrastructure-data/prod/caddy_data:/data
    restart: unless-stopped
    mem_limit: 1g
    mem_reservation: 750m
    env_file:
      - deployment/.env.prod");
            }

            return sb.ToString();
        }
    }
}
