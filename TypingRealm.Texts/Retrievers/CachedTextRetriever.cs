using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Retrievers;

// TODO: Consider not using IServiceCacheProvider here and not referencing Communication assembly.
// Use DI principle instead, and implement caching abstraction in Infrastructure project.
public sealed class CachedTextRetriever : SyncManagedDisposable, ITextRetriever
{
    private const int MinCacheSize = 50;
    private const int FilledCacheSize = 100;
    private readonly ITextRetriever _textRetriever;
    private readonly ITextCache _textCache;
    private Task _fillProcess = Task.CompletedTask;

    private readonly SemaphoreSlim _localLock = new SemaphoreSlim(1, 1);

    public CachedTextRetriever(
        ITextRetriever textRetriever,
        ITextCache textCache)
    {
        _textRetriever = textRetriever;
        _textCache = textCache;
    }

    public string Language => _textRetriever.Language;

    public async ValueTask<string> RetrieveTextAsync()
    {
        ThrowIfDisposed();

        var count = await _textCache.GetCountAsync()
            .ConfigureAwait(false);

        if (count < FilledCacheSize)
        {
            await _localLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_fillProcess.IsCompleted)
                    _fillProcess = FillCacheAsync();
            }
            finally
            {
                _localLock.Release();
            }

            if (count < MinCacheSize)
                return await _textRetriever.RetrieveTextAsync().ConfigureAwait(false);
        }

        var cachedText = await _textCache.GetRandomTextAsync().ConfigureAwait(false);
        if (cachedText == null)
            return await _textRetriever.RetrieveTextAsync().ConfigureAwait(false);

        return cachedText.Value;
    }

    private async Task FillCacheAsync()
    {
        var texts = new HashSet<string>();

        for (var i = 0; i < FilledCacheSize; i++)
        {
            var text = await _textRetriever.RetrieveTextAsync()
                .ConfigureAwait(false);

            var Text = TextGenerator.GetText(text);

            texts.UnionWith(Text);
        }

        await _textCache.AddTextsAsync(texts.Select(value => new CachedText(value)))
            .ConfigureAwait(false);
    }

    protected override void DisposeManagedResources()
    {
        _localLock.Dispose();
    }
}
