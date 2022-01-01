using System;
using System.Text.Json;
using System.Threading;
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

        public IDistributedLock AcquireDistributedLock(TimeSpan expiration)
        {
            return new RedisDistributedLock(_database, _keyPrefix, expiration);
        }

        public async ValueTask<T?> GetValueAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(GetRedisKey(key))
                .ConfigureAwait(false);

            if (!value.HasValue)
                return default; // TODO: Make sure customers of this method expect this behaviour (0 instead of null when T is INT).

            var stringValue = value.ToString();

            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                return (T)Convert.ChangeType(stringValue, typeof(T));

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

    public sealed class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabaseAsync _database;
        private readonly string _keyPrefix;
        private readonly TimeSpan _expirationTime;
        private string? _lockValue;

        public RedisDistributedLock(IDatabaseAsync database, string keyPrefix, TimeSpan expirationTime)
        {
            _database = database;
            _keyPrefix = keyPrefix;
            _expirationTime = expirationTime;
        }

        public async ValueTask ReleaseAsync(CancellationToken cancellationToken)
        {
            if (_lockValue == null)
                throw new InvalidOperationException("Lock has not been acquired yet.");

            var value = await _database.StringGetAsync(GetRedisKey())
                .ConfigureAwait(false);

            if (value != _lockValue)
                throw new InvalidOperationException("Cannot release lock as another thread has acquired it.");

            await _database.KeyDeleteAsync(GetRedisKey())
                .ConfigureAwait(false);
        }

        public async ValueTask WaitAsync(CancellationToken cancellationToken)
        {
            if (_lockValue != null)
                throw new InvalidOperationException("Lock already acquired.");

            _lockValue = Guid.NewGuid().ToString();

            // Simple spinner implementation of lock mechanism.
            while (true)
            {
                var result = await _database.StringSetAsync(
                    GetRedisKey(),
                    _lockValue,
                    when: When.NotExists,
                    expiry: _expirationTime).ConfigureAwait(false);

                if (result)
                    return;

                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }

        private RedisKey GetRedisKey()
        {
            return new RedisKey($"lock_{_keyPrefix}");
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
