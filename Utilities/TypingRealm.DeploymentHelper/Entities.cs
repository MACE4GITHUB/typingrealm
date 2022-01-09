﻿using System;
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

        public bool OnlyMainNetwork => Value == "debug";

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
