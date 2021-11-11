using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public sealed class TextGenerator : ITextGenerator
    {
#pragma warning disable CS8618
        private sealed class QuotableResponse
        {
            public string content { get; set; }
        }
#pragma warning restore CS8618

        private static readonly string _allowedLetters = "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
        private readonly IHttpClientFactory _httpClientFactory;

        public TextGenerator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> GenerateTextAsync(TextConfiguration configuration)
        {
            if (configuration.Length != 0)
                throw new NotImplementedException("You can only request default (zero) text length for now.");

            var minLength = 10;
            var builder = new StringBuilder();

            var client = _httpClientFactory.CreateClient();

            while (builder.Length < minLength)
            {
                var response = await client.GetAsync("http://api.quotable.io/random")
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                var quotableResponse = JsonSerializer.Deserialize<QuotableResponse>(content)?.content;
                if (quotableResponse == null)
                    throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");

                var allowed = true;
                foreach (var letter in quotableResponse)
                {
                    if (!_allowedLetters.Contains(letter))
                    {
                        allowed = false;
                        break;
                    }
                }

                if (allowed)
                    builder.Append(quotableResponse);
            }

            return builder.ToString();
        }
    }
}
