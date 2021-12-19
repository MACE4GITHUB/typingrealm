using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Texts.Generators
{
    public sealed class CachedTextRetriever : SyncManagedDisposable, ITextRetriever
    {
        private const int CacheSize = 100;
        private readonly ITextRetriever _textRetriever;
        private readonly IServiceCacheProvider _cacheProvider;
        private readonly string _cachePrefix;
        private Task _fillProcess = Task.CompletedTask;

        // TODO: Dispose of this.
        private readonly SemaphoreSlim _localLock = new SemaphoreSlim(1, 1);

        public CachedTextRetriever(
            ITextRetriever textRetriever,
            IServiceCacheProvider cacheProvider)
        {
            _textRetriever = textRetriever;
            _cacheProvider = cacheProvider;
            _cachePrefix = $"texts_{_textRetriever.Language}_";
        }

        public string Language => _textRetriever.Language;

        public async ValueTask<string> RetrieveTextAsync()
        {
            ThrowIfDisposed();

            var cache = await GetCacheAsync().ConfigureAwait(false);

            // TODO: Make sure this is hybrid cache: cache results in memory and listen to Redis changes.
            var values = await cache.GetValueAsync<List<string>>(GetCacheKey())
                .ConfigureAwait(false);

            if (values == null || values.Count < CacheSize)
            {
                await _localLock.WaitAsync();
                try
                {
                    if (_fillProcess.IsCompleted)
                        _fillProcess = FillCacheAsync();
                }
                finally
                {
                    _localLock.Release();
                }

                return await _textRetriever.RetrieveTextAsync();
            }

            // TODO: Move out Randomizer to Common project.
            // TODO: Return multiple sentences in one response, not just one.
            var index = RandomNumberGenerator.GetInt32(0, values.Count);
            var text = values[index];
            return text;
        }

        private static string GetCacheKey() => "sentences";

        private async Task FillCacheAsync()
        {
            var texts = new HashSet<string>();

            for (var i = 0; i < CacheSize; i++)
            {
                var text = await _textRetriever.RetrieveTextAsync()
                    .ConfigureAwait(false);

                var sentences = TextGenerator.GetSentences(text);

                texts.UnionWith(sentences);
            }

            var cache = await GetCacheAsync().ConfigureAwait(false);
            var @lock = cache.AcquireDistributedLock();
            await @lock.WaitAsync(default).ConfigureAwait(false);
            try
            {
                var values = await cache.GetValueAsync<List<string>>(GetCacheKey())
                    .ConfigureAwait(false);

                if (values == null)
                    values = new List<string>();

                values.AddRange(texts);
                values = values.Distinct().ToList();

                await cache.SetValueAsync(GetCacheKey(), values);
            }
            finally
            {
                await @lock.ReleaseAsync(default);
            }
        }

        // TODO: Move this method to common helpers as it's also needed in TextGenerator.
        private ValueTask<ITyrCache> GetCacheAsync() => _cacheProvider.GetServiceCacheAsync(_cachePrefix);

        protected override void DisposeManagedResources()
        {
            _localLock.Dispose();
        }
    }
}
