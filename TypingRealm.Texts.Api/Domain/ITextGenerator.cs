using System.Threading.Tasks;

namespace TypingRealm.Texts
{
    public interface ITextGenerator
    {
        string Language { get; }
        ValueTask<string> GenerateTextAsync(TextGenerationConfiguration configuration);
    }

    /// <summary>
    /// Retrieves texts from third-party APIs or cache.
    /// </summary>
    public interface ITextRetriever
    {
        string Language { get; }
        ValueTask<string> RetrieveTextAsync();
    }
}
