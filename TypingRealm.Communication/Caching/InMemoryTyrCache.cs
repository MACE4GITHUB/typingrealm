using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace TypingRealm.Communication;

// Should be singleton and registered only when UseInfrastructure = false.
public sealed class InMemoryTyrCache : SyncManagedDisposable, ITyrCache, IDistributedLockProvider
{
    private readonly ConcurrentDictionary<string, Lazy<SemaphoreSlimLock>> _locks
        = new ConcurrentDictionary<string, Lazy<SemaphoreSlimLock>>();
    private readonly IMemoryCache _cache;

    public InMemoryTyrCache(
        IMemoryCache cache)
    {
        _cache = cache;
    }

    public ILock AcquireDistributedLock(string name, TimeSpan expiration)
    {
        var @lock = _locks.GetOrAdd(GetLockKey(GetLockKey(name)), _ => new(() => new SemaphoreSlimLock()));
        if (@lock.Value.IsDisposed)
        {
            _locks.TryUpdate(GetLockKey(GetLockKey(name)), new(() => new SemaphoreSlimLock()), @lock);
            return AcquireDistributedLock(GetLockKey(name), expiration);
        }

        return @lock.Value;
    }

    public async ValueTask<T?> GetValueAsync<T>(string key)
    {
        _cache.TryGetValue<string>(GetCacheKey(key), out var value);

        return Deserialize<T>(value);
    }

    public async ValueTask MergeCollectionAsync<T>(string key, IEnumerable<T> collection, bool isUnique = true)
    {
        var existing = new List<T>();
        if (_cache.TryGetValue<string>(GetCacheKey(key), out var value))
        {
            existing.AddRange(
                Deserialize<IEnumerable<T>>(value));
        }

        var all = existing.Concat(collection);
        if (isUnique)
            all = all.Distinct();

        _cache.Set(GetCacheKey(key), Serialize(all));
    }

    public async ValueTask SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (expiration == null)
            _cache.Set(GetCacheKey(key), value);
        else
            _cache.Set(GetCacheKey(key), value, expiration.Value);
    }

    private string GetCacheKey(string key) => $"service_cache_{key}";

    // TODO: Move to Common or to some serialization project.
    private bool CanBeConverted<T>() => typeof(T).IsPrimitive || typeof(T) == typeof(string);
    private bool TryConvert<T>(string value, out T convertedValue)
    {
        if (!CanBeConverted<T>())
        {
            convertedValue = default;
            return false;
        }

        convertedValue = (T)Convert.ChangeType(value, typeof(T));
        return true;
    }
    private T Deserialize<T>(string value)
    {
        if (TryConvert<T>(value, out var result))
            return result;

        // TODO: Use camelcase names on serialization and deserialization,
        // move serialization logic to a separate abstraction.
        var deserialized = JsonSerializer.Deserialize<T>(value);
        if (deserialized == null)
            throw new InvalidOperationException("Could not deserialize cached value.");

        return deserialized;
    }
    private string Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    private string GetLockKey(string name)
    {
        return $"lock_{name}";
    }

    protected override void DisposeManagedResources()
    {
        foreach (var @lock in _locks.Values)
        {
            @lock.Value.Dispose();
        }
    }
}
