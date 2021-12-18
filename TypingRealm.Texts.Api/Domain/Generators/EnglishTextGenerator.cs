using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Generators
{
    public sealed class EnglishTextGenerator : ITextGenerator
    {
        private sealed record QuotableResponse(string content);

        public string Language => "en";
        private readonly IHttpClientFactory _httpClientFactory;

        public EnglishTextGenerator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> GenerateTextAsync(TextGenerationConfiguration configuration)
        {
            var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.GetAsync("https://api.quotable.io/random")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var textValue = JsonSerializer.Deserialize<QuotableResponse>(content)?.content;
            if (textValue == null)
                throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");

            return textValue;
        }
    }
}
