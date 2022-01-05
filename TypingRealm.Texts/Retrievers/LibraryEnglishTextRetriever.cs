using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Api.Client;

namespace TypingRealm.Texts.Retrievers;

public sealed class LibraryEnglishTextRetriever : ITextRetriever
{
    private readonly ISentencesClient _client;

    public LibraryEnglishTextRetriever(ISentencesClient client)
    {
        _client = client;
    }

    public string Language => "en";

    public async ValueTask<string> RetrieveTextAsync()
    {
        var sentences = await _client.GetRandomSentencesAsync(100, 1, default)
            .ConfigureAwait(false);

        return string.Join(" ", sentences.Select(s => s.Value));
    }
}
