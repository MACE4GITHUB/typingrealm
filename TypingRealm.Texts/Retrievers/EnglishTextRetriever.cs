using System;
using System.Net.Http;
using System.Text.Json;

namespace TypingRealm.Texts.Retrievers;

public sealed class EnglishTextRetriever : HttpTextRetriever
{
    private sealed record QuotableResponse(string content);

    public EnglishTextRetriever(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "en", new Uri("https://api.quotable.io/random"))
    {
    }

    protected override string AllowedLetters => "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";

    protected override string ResponseHandler(string response)
    {
        var value = JsonSerializer.Deserialize<QuotableResponse>(response)?.content;
        if (value == null)
            throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");

        return value;
    }
}
