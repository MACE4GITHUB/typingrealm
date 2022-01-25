using System;
using System.Threading.Tasks;

namespace TypingRealm.Communication;

public interface ITyrCache
{
    ValueTask<T?> GetValueAsync<T>(string key);

    ValueTask SetValueAsync<T>(string key, T value);

    IDistributedLock AcquireDistributedLock(TimeSpan expiration);
}

public interface IDistributedLock : ILock
{
}

public static class TyrCacheExtensions
{
    public static ValueTask<string?> GetValueAsync(this ITyrCache cache, string key)
        => cache.GetValueAsync<string>(key);

    public static ValueTask SetValueAsync(this ITyrCache cache, string key, string value)
        => cache.SetValueAsync<string>(key, value);

    /// <summary>
    /// Acquires distributed lock with default expiration time of 1 hour.
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public static IDistributedLock AcquireDistributedLock(this ITyrCache cache)
        => cache.AcquireDistributedLock(TimeSpan.FromHours(1));
}

public interface IGlobalCacheProvider
{
    ValueTask<ITyrCache> GetGlobalCacheAsync(string keyPrefix = "");
}

public interface IServiceCacheProvider
{
    ValueTask<ITyrCache> GetServiceCacheAsync(string keyPrefix = "");
}
