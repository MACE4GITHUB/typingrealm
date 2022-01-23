using System.Collections.Generic;
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

    public async ValueTask<string> RetrieveTextAsync(IEnumerable<string>? contains = null)
    {
        var request = SentencesRequest.Random(100, 2);
        if (contains != null && contains.Any())
        {
            request.Contains = contains;
            request.Type = SentencesRequestType.ContainingKeyPairs;
            request.ConsecutiveCount = 1;
        }

        var sentences = await _client.GetSentencesAsync(request, Language)
            .ConfigureAwait(false);

        return string.Join(" ", sentences.Select(s => s.Value));
    }
}
