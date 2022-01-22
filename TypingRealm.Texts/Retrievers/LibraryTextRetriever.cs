using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Api.Client;
using TypingRealm.Library.Sentences;

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
        var sentences = await _client.GetSentencesAsync(
            SentencesRequest.Random(100, 2), Language)
            .ConfigureAwait(false);

        return string.Join(" ", sentences.Select(s => s.Value));
    }
}
