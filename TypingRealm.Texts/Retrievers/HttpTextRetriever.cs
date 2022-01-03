using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Retrievers;

public abstract class HttpTextRetriever : ITextRetriever
{
    private const int RetriesCount = 10;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _apiUri;

    protected HttpTextRetriever(
        IHttpClientFactory httpClientFactory,
        string language,
        Uri apiUri)
    {
        _httpClientFactory = httpClientFactory;
        Language = language;
        _apiUri = apiUri;
    }

    public string Language { get; }
    protected abstract string AllowedLetters { get; }

    public async ValueTask<string> RetrieveTextAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var count = 0; // Avoid endless loops.
        while (count < RetriesCount)
        {
            count++;

            using var response = await httpClient.GetAsync(_apiUri)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var text = ResponseHandler(content);

            if (string.IsNullOrWhiteSpace(text) || !text.All(character => AllowedLetters.Contains(character)))
                continue;

            return text;
        }

        throw new InvalidOperationException("Could not get text within allowed characters limit.");
    }

    protected abstract string ResponseHandler(string response);
}
