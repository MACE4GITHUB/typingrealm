using System.Linq;
using System.Text;

namespace TypingRealm.DeploymentHelper;

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
