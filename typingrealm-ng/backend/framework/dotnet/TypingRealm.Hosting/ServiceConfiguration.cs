namespace TypingRealm.Hosting;

public sealed record ServiceConfiguration(
    string[] CorsOrigins,
    string DbConnectionString,
    string CacheConnectionString);
