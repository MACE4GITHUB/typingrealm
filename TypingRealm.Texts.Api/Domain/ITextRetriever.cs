using System.Threading.Tasks;

namespace TypingRealm.Texts;

/// <summary>
/// Retrieves texts from third-party APIs or cache.
/// </summary>
public interface ITextRetriever
{
    string Language { get; }
    ValueTask<string> RetrieveTextAsync();
}
