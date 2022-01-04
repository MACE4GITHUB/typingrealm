using System;
using System.Net.Http;
using System.Text.Json;

namespace TypingRealm.Texts.Retrievers;

public sealed class EnglishTextRetriever : HttpTextRetriever
{
    private sealed record QuotableResponse(string content);

    public EnglishTextRetriever(IHttpClientFactory httpClientFactory)
#pragma warning disable S1075 // URIs should not be hardcoded: this is a specific text retriever working with this exact URI and no other.
        : base(httpClientFactory, "en", new Uri("https://api.quotable.io/random"))
#pragma warning restore S1075
    {
    }

    protected override string AllowedLetters => TextHelpers.AllowedEnglishLetters;

    protected override string ResponseHandler(string response)
    {
        var value = JsonSerializer.Deserialize<QuotableResponse>(response)?.content;

        return value ?? throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");
    }
}
