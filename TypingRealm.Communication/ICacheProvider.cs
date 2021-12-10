using System.Threading.Tasks;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.Communication
{
    public interface ITyrCache
    {
        ValueTask<T?> GetValueAsync<T>(string key)
            where T : class;

        ValueTask SetValueAsync<T>(string key, T value);

        IDistributedLock AcquireDistributedLock();
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
    }

    public interface IGlobalCacheProvider
    {
        ValueTask<ITyrCache> GetGlobalCacheAsync(string keyPrefix = "");
    }

    public interface IServiceCacheProvider
    {
        ValueTask<ITyrCache> GetServiceCacheAsync(string keyPrefix = "");
    }
}
