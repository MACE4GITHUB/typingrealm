using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Generators
{
    public sealed class RussianTextRetriever : ITextRetriever
    {
        public string Language => "ru";
        private readonly IHttpClientFactory _httpClientFactory;

        public RussianTextRetriever(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> RetrieveTextAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.GetAsync("https://fish-text.ru/get?format=html")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var value = content.Replace("<p>", string.Empty).Replace("</p>", string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Text API returned empty text.");

            return value;
        }
    }
}
