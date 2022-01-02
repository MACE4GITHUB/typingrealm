using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Retrievers;

public sealed class CachedTextRetriever : AsyncManagedDisposable, ITextRetriever
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
                    _fillProcess = FillCacheAsync(); // TODO: Consider logging exceptions.
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
        var uniqueSentences = new HashSet<string>();

        for (var i = 0; i < FilledCacheSize; i++)
        {
            var text = await _textRetriever.RetrieveTextAsync()
                .ConfigureAwait(false);

            uniqueSentences.UnionWith(TextGenerator.GetSentencesEnumerable(text));
        }

        await _textCache.AddTextsAsync(uniqueSentences.Select(sentence => new CachedText(sentence)))
            .ConfigureAwait(false);
    }

    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        await _fillProcess.HandleExceptionAsync<Exception>(exception => { /* Consider logging it. */ })
            .ConfigureAwait(false);

        _localLock.Dispose();
    }
}
