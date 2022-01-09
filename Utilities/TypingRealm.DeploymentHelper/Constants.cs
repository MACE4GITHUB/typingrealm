using TypingRealm.DeploymentHelper.Data;

namespace TypingRealm.DeploymentHelper;

public static class Constants
{
    public const string ProjectName = "typingrealm";
    public const string Domain = $"{ProjectName}.com";
    public const string LocalDomain = "localhost";
    public const string Email = $"{ProjectName}@gmail.com";
    public const string InfrastructureDataFolder = "./infrastructure-data";
    public const string WebUiServiceName = "web-ui";
    public const string AuthorityServiceName = "identityserver";
    public const string EnvironmentFilesFolderWithSlash = "deployment/";

    public const string CacheConfigurationKey = "ConnectionStrings__ServiceCacheConnection";
    public const string DatabaseConfigurationKey = "ConnectionStrings__DataConnection";
    public const string PostgresUserId = "postgres";
    public const string PostgresPassword = "admin";
    public const string PostgresDatabase = "db";

    // Web UI.
    public const string WebUiDockerPath = $"{ProjectName}-{WebUiServiceName}:80";
    public const string LocalWebUiDockerPath = "host.docker.internal:4200";

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
