using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TypingRealm.Serialization;

namespace TypingRealm.Communication;

// Should be singleton and registered only when UseInfrastructure = false.
public sealed class InMemoryTyrCache : SyncManagedDisposable, ITyrCache, IDistributedLockProvider
{
    private readonly ConcurrentDictionary<string, Lazy<SemaphoreSlimLock>> _locks
        = new ConcurrentDictionary<string, Lazy<SemaphoreSlimLock>>();
    private readonly IMemoryCache _cache;
    private readonly ISerializer _serializer;

    public InMemoryTyrCache(
        IMemoryCache cache,
        ISerializer serializer)
    {
        _cache = cache;
        _serializer = serializer;
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

        return _serializer.Deserialize<T>(value);
    }

    public async ValueTask MergeCollectionAsync<T>(string key, IEnumerable<T> collection, bool isUnique = true)
    {
        var existing = new List<T>();
        if (_cache.TryGetValue<string>(GetCacheKey(key), out var value))
        {
            existing.AddRange(
                _serializer.Deserialize<IEnumerable<T>>(value) ?? throw new InvalidOperationException($"Could not deserialize collection from key {key}."));
        }

        var all = existing.Concat(collection);
        if (isUnique)
            all = all.Distinct();

        _cache.Set(GetCacheKey(key), _serializer.Serialize(all));
    }

    public async ValueTask SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (expiration == null)
            _cache.Set(GetCacheKey(key), _serializer.Serialize(value));
        else
            _cache.Set(GetCacheKey(key), _serializer.Serialize(value), expiration.Value);
    }

    private string GetCacheKey(string key) => $"service_cache_{key}";

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
