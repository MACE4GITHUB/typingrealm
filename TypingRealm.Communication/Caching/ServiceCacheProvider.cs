using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace TypingRealm.Communication;

// Should be singletone as the lock should be shared between all InMemoryTyrCache instances.
public sealed class ServiceCacheProvider : SyncManagedDisposable, IServiceCacheProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly SemaphoreSlimLock _distributedLock;

    public ServiceCacheProvider(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _distributedLock = new SemaphoreSlimLock();
    }

    public async ValueTask<ITyrCache> GetServiceCacheAsync(string keyPrefix = "")
    {
        return new InMemoryTyrCache(_memoryCache, _distributedLock, keyPrefix);
    }

    protected override void DisposeManagedResources()
    {
        // After disposal we cannot use any instances of InMemoryTyrCache issued by this provider.
        _distributedLock.Dispose();
    }
}
