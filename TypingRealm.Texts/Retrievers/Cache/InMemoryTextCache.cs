using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Retrievers.Cache;

/// <summary>
/// Will not be distributed.
/// </summary>
public sealed class InMemoryTextCache : ITextCache
{
    private readonly ConcurrentDictionary<string, CachedText> _texts
        = new ConcurrentDictionary<string, CachedText>();

    public InMemoryTextCache(string language)
    {
        Language = language;
    }

    public string Language { get; }

    public ValueTask AddTextsAsync(IEnumerable<CachedText> texts)
    {
        foreach (var text in texts)
        {
            _texts.TryAdd(text.Value, text);
        }

        return default;
    }

    public ValueTask<int> GetCountAsync()
    {
        return new ValueTask<int>(_texts.Count);
    }

    public ValueTask<CachedText> GetRandomTextAsync()
    {
        while (true)
        {
            var index = RandomNumberGenerator.GetInt32(0, _texts.Count);
            var text = _texts.Values.ElementAtOrDefault(index);
            if (text == null)
                continue; // In case some texts were removed (by future expiration feature).

            return new ValueTask<CachedText>(text);
        }
    }
}
