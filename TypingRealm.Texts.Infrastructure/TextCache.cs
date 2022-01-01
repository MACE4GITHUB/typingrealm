using System.Security.Cryptography;
using TypingRealm.Communication;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Infrastructure
{
    public sealed class TextCache : ITextCache
    {
        private readonly IServiceCacheProvider _serviceCacheProvider;

        public TextCache(IServiceCacheProvider serviceCacheProvider, string language)
        {
            _serviceCacheProvider = serviceCacheProvider;
            Language = language;
        }

        public string Language { get; }

        public async ValueTask AddTextsAsync(IEnumerable<CachedText> texts)
        {
            var cache = await GetCacheAsync()
                .ConfigureAwait(false);

            var @lock = cache.AcquireDistributedLock();
            await using var _ = (await @lock.UseWaitAsync(default).ConfigureAwait(false)).ConfigureAwait(false);

            var values = await cache.GetValueAsync<List<CachedText>>("texts")
                .ConfigureAwait(false);

            if (values == null)
                values = new List<CachedText>();

            var newValues = values.Concat(texts)
                .DistinctBy(x => x.Value)
                .ToList();

            await cache.SetValueAsync("count", newValues.Count)
                .ConfigureAwait(false);

            await cache.SetValueAsync("texts", newValues)
                .ConfigureAwait(false);
        }

        public async ValueTask<int> GetCountAsync()
        {
            var cache = await GetCacheAsync()
                .ConfigureAwait(false);

            return await cache.GetValueAsync<int>("count")
                .ConfigureAwait(false);
        }

        public async ValueTask<CachedText?> GetRandomTextAsync()
        {
            var cache = await GetCacheAsync()
                .ConfigureAwait(false);

            var values = await cache.GetValueAsync<List<CachedText>>("texts")
                .ConfigureAwait(false);

            if (values == null)
                return null;

            var index = RandomNumberGenerator.GetInt32(0, values.Count);
            return values[index];
        }

        private ValueTask<ITyrCache> GetCacheAsync()
        {
            return _serviceCacheProvider.GetServiceCacheAsync($"texts_{Language}");
        }
    }
}
