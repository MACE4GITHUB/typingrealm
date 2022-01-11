using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Api.Client;

namespace TypingRealm.Texts.Retrievers;

public sealed class LibraryTextRetriever : ITextRetriever
{
    private readonly ISentencesClient _client;

    public LibraryTextRetriever(ISentencesClient client, string language)
    {
        _client = client;
        Language = language;
    }

    public string Language { get; }

    public async ValueTask<string> RetrieveTextAsync()
    {
        var sentences = await _client.GetRandomSentencesAsync(Language, 100, 2, default)
            .ConfigureAwait(false);

        return string.Join(" ", sentences.Select(s => s.Value));
    }
}
