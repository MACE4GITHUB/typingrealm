using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace TypingRealm.Communication.Redis;

public static class RegistrationExtensions
{
    public static IServiceCollection TryAddRedisServiceCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceCacheConnectionString = configuration.GetConnectionString("ServiceCacheConnection");
        if (serviceCacheConnectionString == null)
            return services;

        return services.AddRedisServiceCaching(serviceCacheConnectionString);
    }

    public static IServiceCollection AddRedisServiceCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceCacheConnectionString = configuration.GetConnectionString("ServiceCacheConnection");
        if (serviceCacheConnectionString == null)
            throw new InvalidOperationException("ServiceCacheConnection connection string is not set.");

        return services.AddRedisServiceCaching(serviceCacheConnectionString);
    }

    public static IServiceCollection AddRedisServiceCaching(
        this IServiceCollection services,
        string serviceCacheConnectionString)
    {
        ArgumentNullException.ThrowIfNull(serviceCacheConnectionString);

        services.AddSingleton<RedisTyrCache>(provider =>
        {
            // TODO: Figure out how to do this asynchronously.
            var multiplexer = ConnectionMultiplexer.Connect(serviceCacheConnectionString);
            return new RedisTyrCache(multiplexer);
        });

        services.AddSingleton<ITyrCache>(
            provider => provider.GetRequiredService<RedisTyrCache>());
        services.AddSingleton<IDistributedLockProvider>(
            provider => provider.GetRequiredService<RedisTyrCache>());

        return services;
    }
}
