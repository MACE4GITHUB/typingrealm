using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Generators;

public sealed class EnglishTextRetriever : ITextRetriever
{
    private sealed record QuotableResponse(string content);

    private const int RetriesCount = 10;
    private static readonly string _allowedLetters = "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
    private readonly IHttpClientFactory _httpClientFactory;

    public EnglishTextRetriever(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string Language => "en";

    public async ValueTask<string> RetrieveTextAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var count = 0; // Avoid endless loops.
        while (count < RetriesCount)
        {
            count++;

            using var response = await httpClient.GetAsync("https://api.quotable.io/random")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var textValue = JsonSerializer.Deserialize<QuotableResponse>(content)?.content;
            if (textValue == null)
                throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");

            if (!textValue.All(character => _allowedLetters.Contains(character)))
                continue;

            return textValue;
        }

        throw new InvalidOperationException("Could not get text within allowed characters limit.");
    }
}
