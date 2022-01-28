using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace TypingRealm.Communication;

public sealed class InMemoryTyrCache : ITyrCache
{
    private readonly ILock _distributedLock;
    private readonly string _keyPrefix;
    private readonly IMemoryCache _cache;

    public InMemoryTyrCache(
        IMemoryCache cache,
        ILock distributedLock,
        string keyPrefix)
    {
        _cache = cache;
        _distributedLock = distributedLock;
        _keyPrefix = $"service_cache_{keyPrefix}";
    }

    public ILock AcquireDistributedLock(TimeSpan expiration)
    {
        return _distributedLock;
    }

    public async ValueTask<T?> GetValueAsync<T>(string key)
    {
        _cache.TryGetValue<string>(GetKey(key), out var value);

        return Deserialize<T>(value);
    }

    public async ValueTask MergeCollectionAsync<T>(string key, IEnumerable<T> collection, bool isUnique = true)
    {
        var existing = new List<T>();
        if (_cache.TryGetValue<string>(GetKey(key), out var value))
        {
            existing.AddRange(
                Deserialize<IEnumerable<T>>(value));
        }

        var all = existing.Concat(collection);
        if (isUnique)
            all = all.Distinct();

        _cache.Set(GetKey(key), Serialize(all));
    }

    public async ValueTask SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (expiration == null)
            _cache.Set(GetKey(key), value);
        else
            _cache.Set(GetKey(key), value, expiration.Value);
    }

    private string GetKey(string key) => $"{_keyPrefix}_{key}";

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
}
