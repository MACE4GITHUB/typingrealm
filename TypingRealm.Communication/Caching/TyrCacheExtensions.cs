using System;
using System.Threading.Tasks;

namespace TypingRealm.Communication;

public static class TyrCacheExtensions
{
    public static ValueTask<string?> GetValueAsync(this ITyrCache cache, string key)
        => cache.GetValueAsync<string>(key);

    /// <summary>
    /// Acquires distributed lock with default expiration time of 1 hour.
    /// </summary>
    public static ILock AcquireDistributedLock(this ITyrCache cache)
        => cache.AcquireDistributedLock(TimeSpan.FromHours(1));
}
