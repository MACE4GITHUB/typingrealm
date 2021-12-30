using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Retrievers.Cache;

/// <summary>
/// Should add texts atomically from different pods.
/// </summary>
public interface ITextCache
{
    string Language { get; }

    ValueTask<int> GetCountAsync();
    ValueTask<CachedText?> GetRandomTextAsync();
    ValueTask AddTextsAsync(IEnumerable<CachedText> texts);
}
