using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Api;

public interface ITextRetriever
{
    ValueTask<string> RetrieveTextAsync();
}

public sealed class TextRetriever : ITextRetriever
{
    private sealed record QuotableResponse(string content);

    private readonly IHttpClientFactory _httpClientFactory;

    public TextRetriever(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask<string> RetrieveTextAsync()
    {
        using var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync("https://api.quotable.io/random");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var textObject = JsonSerializer.Deserialize<QuotableResponse>(content);
        if (textObject == null)
            throw new InvalidOperationException("Could not retrieve text from quotable.io.");

        return textObject.content;
    }
}
