using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Retrievers;

public sealed class CachedTextRetriever : AsyncManagedDisposable, ITextRetriever
{
    private const int MinCacheSize = 50;
    private const int FilledCacheSize = 100;
    private readonly ILogger<CachedTextRetriever> _logger;
    private readonly ITextRetriever _textRetriever;
    private readonly ITextCache _textCache;
    private Task _fillProcess = Task.CompletedTask;

    private readonly SemaphoreSlim _localLock = new(1, 1);

    public CachedTextRetriever(
        ILogger<CachedTextRetriever> logger,
        ITextRetriever textRetriever,
        ITextCache textCache)
    {
        _logger = logger;
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
                    _fillProcess = FillCacheAsync().HandleExceptionAsync<Exception>(exception =>
                    {
                        _logger.LogWarning(exception, $"Error while filling cache in background for {nameof(CachedTextRetriever)} ({Language} language).");
                    });
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
            try
            {
                var text = await _textRetriever.RetrieveTextAsync()
                    .ConfigureAwait(false);

                uniqueSentences.UnionWith(TextGenerator.GetSentencesEnumerable(text));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error while trying to receive a text in {nameof(FillCacheAsync)} method, stopping filling cache ({Language} language).");
                break;
            }
        }

        if (uniqueSentences.Count == 0)
            return;

        await _textCache.AddTextsAsync(uniqueSentences.Select(sentence => new CachedText(sentence)))
            .ConfigureAwait(false);

        _logger.LogInformation($"Filled cache for '{Language}' language with {uniqueSentences.Count} unique sentences.");
    }

    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        await _fillProcess.HandleExceptionAsync<Exception>(exception =>
        {
            _logger.LogWarning(exception, $"Error while waiting for {nameof(_fillProcess)} when disposing resources in {nameof(CachedTextRetriever)} ({Language} language.");
        }).ConfigureAwait(false);

        _localLock.Dispose();
    }
}
