using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace TypingRealm.Communication.Redis
{
    public sealed class GlobalCacheProvider : IGlobalCacheProvider
    {
        private readonly string _cacheConnectionString;

        public GlobalCacheProvider(string cacheConnectionString)
        {
            _cacheConnectionString = cacheConnectionString;
        }

        public async ValueTask<ITyrCache> GetGlobalCacheAsync(string keyPrefix = "")
        {
            // TODO: Implement some kind of multiplexer pool.

            var multiplexer = await ConnectionMultiplexer.ConnectAsync(_cacheConnectionString)
                .ConfigureAwait(false);

            var cache = new RedisTyrCache(multiplexer, keyPrefix);

            return cache;
        }
    }

    public sealed class ServiceCacheProvider : IServiceCacheProvider
    {
        private readonly string _cacheConnectionString;

        public ServiceCacheProvider(string cacheConnectionString)
        {
            _cacheConnectionString = cacheConnectionString;
        }

        public async ValueTask<ITyrCache> GetServiceCacheAsync(string keyPrefix = "")
        {
            // TODO: Implement some kind of multiplexer pool.

            var multiplexer = await ConnectionMultiplexer.ConnectAsync(_cacheConnectionString)
                .ConfigureAwait(false);

            var cache = new RedisTyrCache(multiplexer, keyPrefix);

            return cache;
        }
    }

    // TODO: Move serialization code to shared interface.
    public sealed class RedisTyrCache : ITyrCache
    {
        private readonly IDatabaseAsync _database;
        private readonly string _keyPrefix;

        public RedisTyrCache(
            IConnectionMultiplexer multiplexer,
            string keyPrefix)
        {
            _database = multiplexer.GetDatabase();
            _keyPrefix = keyPrefix;
        }

        public async ValueTask<T?> GetValueAsync<T>(string key)
            where T : class
        {
            var value = await _database.StringGetAsync(GetRedisKey(key))
                .ConfigureAwait(false);

            if (!value.HasValue)
                return null;

            var stringValue = value.ToString();

            if (typeof(T) == typeof(string))
                return stringValue as T;

            var deserialized = JsonSerializer.Deserialize<T>(stringValue);
            if (deserialized == null)
                throw new InvalidOperationException("Could not deserialize cached value.");

            return deserialized;
        }

        public async ValueTask SetValueAsync<T>(string key, T value)
        {
            if (typeof(T) == typeof(string))
            {
                await _database.StringSetAsync(GetRedisKey(key), value as string)
                    .ConfigureAwait(false);

                return;
            }

            var serialized = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(GetRedisKey(key), serialized)
                .ConfigureAwait(false);
        }

        private RedisKey GetRedisKey(string key)
        {
            return new RedisKey($"{_keyPrefix}_{key}");
        }
    }

    public static class RegistrationExtensions
    {
        public static IServiceCollection TryAddRedisGlobalCaching(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var cacheConnectionString = configuration.GetConnectionString("CacheConnection");
            if (cacheConnectionString == null)
                return services;

            services.AddSingleton<IGlobalCacheProvider>(
                _ => new GlobalCacheProvider(cacheConnectionString));

            return services;
        }

        public static IServiceCollection AddRedisGlobalCaching(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var cacheConnectionString = configuration.GetConnectionString("CacheConnection");
            if (cacheConnectionString == null)
                throw new InvalidOperationException("CacheConnection connection string is not set.");

            TryAddRedisGlobalCaching(services, configuration);

            return services;
        }

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

            services.AddSingleton<IServiceCacheProvider>(
                _ => new ServiceCacheProvider(serviceCacheConnectionString));

            return services;
        }
    }
}
