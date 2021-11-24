using System;
using System.Linq;
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
            if (configuration.Length < 0)
                throw new InvalidOperationException("Cannot have negative length.");

            var minLength = configuration.Length == 0
                ? 10 // Default value.
                : configuration.Length;

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

                var chunks = configuration.TextType == TextType.Words
                    ? quotableResponse.Split(' ')
                    : new[] { quotableResponse };

                // TODO: Account for spaces.
                foreach (var chunk in chunks)
                {
                    var allowed = true;
                    foreach (var letter in chunk)
                    {
                        if (!_allowedLetters.Contains(letter))
                        {
                            allowed = false;
                            break;
                        }
                    }

                    if (configuration.ShouldContain.Any())
                    {
                        allowed = false;
                    }

                    static bool ChunkHasKeyPair(string chunk, string keyPair)
                    {
                        if (chunk.Contains(keyPair))
                            return true;

                        if ((keyPair[0] == ' ' && chunk.StartsWith(keyPair[1]))
                            || (keyPair[1] == ' ' && chunk.EndsWith(keyPair[0])))
                            return true;

                        return false;
                    }

                    foreach (var keyPair in configuration.ShouldContain)
                    {
                        if (ChunkHasKeyPair(chunk, keyPair))
                        {
                            allowed = true;
                            break;
                        }
                    }

                    if (allowed)
                    {
                        if (builder.Length > 0)
                            builder.Append(" ");

                        builder.Append(chunk);

                        if (builder.Length >= minLength)
                            return builder.ToString();
                    }
                }
            }

            return builder.ToString();
        }
    }
}
