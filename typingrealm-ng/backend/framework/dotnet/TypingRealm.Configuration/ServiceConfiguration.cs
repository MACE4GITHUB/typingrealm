namespace TypingRealm.Configuration;

public interface IServiceConfiguration
{
    string[] CorsOrigins { get; }
    string DbConnectionString { get; }
    string CacheConnectionString { get; }
}

public sealed record ServiceConfiguration(
    string[] CorsOrigins,
    string DbConnectionString,
    string CacheConnectionString) : IServiceConfiguration;
