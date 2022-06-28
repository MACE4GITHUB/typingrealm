using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TypingRealm.Communication.Redis;

// TODO: Move serialization code to shared interface.
// TODO: Make sure the instance of this cache is singleton as it opens a connection and has a lock dictionary.
public sealed class RedisTyrCache : ITyrCache, IDistributedLockProvider
{
    private readonly IDatabaseAsync _database;

    public RedisTyrCache(
        IConnectionMultiplexer multiplexer)
    {
        _database = multiplexer.GetDatabase();
    }

    public ILock AcquireDistributedLock(string name, TimeSpan expiration)
    {
        return new RedisDistributedLock(_database, name, expiration);
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

    public async ValueTask MergeCollectionAsync<T>(string key, IEnumerable<T> collection, bool isUnique = true)
    {
        var existing = await GetValueAsync<IEnumerable<T>>(GetRedisKey(key))
            .ConfigureAwait(false);

        if (existing == null)
            existing = new List<T>();

        var all = existing.Concat(collection);
        if (isUnique)
            all = all.Distinct();

        await SetValueAsync(key, all)
            .ConfigureAwait(false);
    }

    public async ValueTask<T?> PopValueAsync<T>(string key)
    {
        var value = await _database.StringGetDeleteAsync(GetRedisKey(key))
            .ConfigureAwait(false);

        // TODO: Refactor this code as it repeats in GetValueAsync method.
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

    public async ValueTask RemoveValueAsync(string key)
    {
        await _database.KeyDeleteAsync(GetRedisKey(key))
            .ConfigureAwait(false);
    }

    public async ValueTask SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (typeof(T) == typeof(string))
        {
            await _database.StringSetAsync(GetRedisKey(key), value as string, expiry: expiration)
                .ConfigureAwait(false);

            return;
        }

        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(GetRedisKey(key), serialized, expiry: expiration)
            .ConfigureAwait(false);
    }

    private string GetRedisKey(string key)
    {
        return $"service_cache_{key}";
    }
}
