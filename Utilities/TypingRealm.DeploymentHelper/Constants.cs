using System.Collections.Generic;
using TypingRealm.DeploymentHelper.Data;

namespace TypingRealm.DeploymentHelper;

public static class Constants
{
    public const string DockerComposeVersion = "3.4";
    public const string RawProjectName = "TypingRealm";
    public static string ProjectName => RawProjectName.ToLowerInvariant();
    public static string Domain => $"{ProjectName}.com";
    public const string LocalDomain = "localhost";
    public static string Email => $"{ProjectName}@gmail.com";
    public const string InfrastructureDataFolder = "./infrastructure-data";
    public const string WebUiServiceName = "web-ui";
    public const string AuthorityServiceName = "IdentityServer";
    public const string EnvironmentFilesFolderWithSlash = "deployment/";

    public const string CacheConfigurationKey = "ConnectionStrings__ServiceCacheConnection";
    public const string DatabaseConfigurationKey = "ConnectionStrings__DataConnection";
    public const string LoggingConfigurationKey = "ConnectionStrings__Logging";
    public const string PostgresUserId = "postgres";
    public const string PostgresPassword = "admin";
    public const string PostgresDatabase = "db";

    // Web UI.
    public static string WebUiDockerPath => $"{ProjectName}-{WebUiServiceName}:80";
    public const string LocalWebUiDockerPath = "host.docker.internal:4200";

    public static string CommunicationProjectName => $"{RawProjectName}.Communication";
    public static string HostingProjectName => $"{RawProjectName}.Hosting";

    public static IEnumerable<string> ExternalNetworksForHostProdCompose => new[]
    {
        "local-tyr_local-typingrealm-net",
        "dev-tyr_dev-typingrealm-net"
    };

    public static IEnumerable<string> InfraExternalNetworks => new[]
    {
        "infra-tyr_typingrealm-infra-net"
    };

    public static string GetReverseProxyAddressWithPort(Service service, string prefix)
    {
        var baseServicePath = GetBaseDockerServicePathWithPort(service);

        return $"{prefix}{baseServicePath}";
    }

    public static string GetBaseDockerServicePathWithPort(Service service)
    {
        return $"{GetBaseDockerServicePath(service)}:80";
    }

    public static string GetBaseDockerServicePath(Service service)
    {
        return $"{ProjectName}-{service.ServiceName}";
    }
}
