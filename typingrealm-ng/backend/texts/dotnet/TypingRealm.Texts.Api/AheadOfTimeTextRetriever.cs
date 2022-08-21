using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Texts.Api;

public sealed class AheadOfTimeTextRetriever : ITextRetriever, IDisposable
{
    private const int MaxSize = 100;
    private readonly ILogger<AheadOfTimeTextRetriever> _logger;
    private readonly ITextRetriever _retriever;
    private readonly ConcurrentQueue<string> _cache = new ConcurrentQueue<string>();
    private readonly Timer _timer;

    public AheadOfTimeTextRetriever(
        ILogger<AheadOfTimeTextRetriever> logger,
        ITextRetriever retriever)
    {
        _logger = logger;
        _retriever = retriever;

        // TODO: Figure out whether I can do async/await here.
        _timer = new Timer(async e => await GetNextTextAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    public ValueTask<string> RetrieveTextAsync()
    {
        if (_cache.TryDequeue(out var value))
            return new(value);

        return _retriever.RetrieveTextAsync();
    }

    private async ValueTask GetNextTextAsync()
    {
        if (_cache.Count >= MaxSize)
            return;

        _cache.Enqueue(await _retriever.RetrieveTextAsync());
        _logger.LogInformation("Got next text.");
    }

    public void Dispose()
    {
        // TODO: Implement Disposable using my primitives helpers.
        _timer.Dispose();
    }
}
