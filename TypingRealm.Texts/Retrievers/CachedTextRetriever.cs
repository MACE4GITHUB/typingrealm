using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.TextProcessing;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Retrievers;

public sealed class CachedTextRetriever : AsyncManagedDisposable, ITextRetriever
{
    private const int MinCacheSize = 50;
    private const int FilledCacheSize = 100;
    private readonly ILogger<CachedTextRetriever> _logger;
    private readonly ITextRetriever _textRetriever;
    private readonly ITextCache _textCache;
    private readonly ITextProcessor _textProcessor;
    private Task _fillProcess = Task.CompletedTask;

    private readonly SemaphoreSlim _localLock = new(1, 1);

    public CachedTextRetriever(
        ILogger<CachedTextRetriever> logger,
        ITextRetriever textRetriever,
        ITextCache textCache,
        ITextProcessor textProcessor)
    {
        _logger = logger;
        _textRetriever = textRetriever;
        _textCache = textCache;
        _textProcessor = textProcessor;
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
                        _logger.LogWarning(
                            exception,
                            "Error while filling text cache ({Language} language).",
                            Language);
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

        return cachedText == null
            ? await _textRetriever.RetrieveTextAsync().ConfigureAwait(false)
            : cachedText.Value;
    }

    private async Task FillCacheAsync()
    {
        var uniqueSentences = new HashSet<string>();

        Exception? retrievalException = null;
        for (var i = 0; i < FilledCacheSize; i++)
        {
            try
            {
                var text = await _textRetriever.RetrieveTextAsync()
                    .ConfigureAwait(false);

                uniqueSentences.UnionWith(_textProcessor.GetSentencesEnumerable(text));
            }
#pragma warning disable CA1031 // It is being re-thrown later on.
            catch (Exception exception)
#pragma warning restore CA1031
            {
                _logger.LogError(
                    exception,
                    "Error while trying to receive a text. Stopping filling cache ({Language} language).",
                    Language);

                retrievalException = exception;
                break;
            }
        }

        if (uniqueSentences.Count == 0)
            return;

        await _textCache.AddTextsAsync(uniqueSentences.Select(sentence => new CachedText(sentence)))
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Filled cache for '{Language}' language with {UniqueSentencesCount} unique sentences.",
            Language, uniqueSentences.Count);

        if (retrievalException != null)
            throw new InvalidOperationException("Error while retrieving text happened during filling the cache.", retrievalException);
    }

    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        await _fillProcess.HandleExceptionAsync<Exception>(exception =>
        {
            _logger.LogWarning(
                exception,
                "Error while waiting for ending filling cache when disposing resources ({Language} language).",
                Language);
        }).ConfigureAwait(false);

        _localLock.Dispose();
    }
}
