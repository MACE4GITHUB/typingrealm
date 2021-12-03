using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    /// <summary>
    /// Constantly gets new texts and caches result.
    /// </summary>
    public sealed class TextGeneratorCache
    {
#pragma warning disable CS8618
        private sealed class QuotableResponse
        {
            public string content { get; set; }
        }
#pragma warning restore CS8618

        private const int CacheSize = 1000;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConcurrentQueue<string> _quoteQueue = new ConcurrentQueue<string>();
        private Task _process;

        public TextGeneratorCache(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _process = StartGettingQuotesAsync();
        }

        public async ValueTask<string> GetQuoteAsync()
        {
            if (_process.IsCompleted && _quoteQueue.Count < CacheSize)
                _process = StartGettingQuotesAsync();

            while (true)
            {
                var quote = await TryGetQuoteAsync()
                    .ConfigureAwait(false);

                if (quote != null)
                    return quote;

                await Task.Delay(10)
                    .ConfigureAwait(false);
            }
        }

        public async ValueTask<string?> TryGetQuoteAsync()
        {
            if (_quoteQueue.TryDequeue(out var quote))
                return quote;

            return null;
        }

        public async Task StartGettingQuotesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();

            while (_quoteQueue.Count < CacheSize)
            {
                using var response = await httpClient.GetAsync("http://api.quotable.io/random")
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                var quotableResponse = JsonSerializer.Deserialize<QuotableResponse>(content)?.content;
                if (quotableResponse == null)
                    throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");

                _quoteQueue.Enqueue(quotableResponse);
            }
        }
    }

    public sealed class TextGenerator : ITextGenerator
    {
        private const int MaxTextLength = 5000; // So that user cannot request 50000 characters.
        private const int ShouldContainMinCount = 15; // If there's not enough data - generate default text.

        private static readonly string _allowedLetters = "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
        private readonly TextGeneratorCache _cache;

        public TextGenerator(IHttpClientFactory httpClientFactory)
        {
            _cache = new TextGeneratorCache(httpClientFactory);
        }

        public async ValueTask<string> GenerateTextAsync(TextConfiguration configuration)
        {
            if (configuration.Length < 0)
                throw new InvalidOperationException("Cannot have negative length.");

            var minLength = configuration.Length == 0
                ? 10 // Default value.
                : configuration.Length;

            var builder = new StringBuilder();

            while (builder.Length < Math.Min(minLength, MaxTextLength))
            {
                var quoteFromApi = await _cache.GetQuoteAsync()
                    .ConfigureAwait(false);

                var chunks = configuration.TextType == TextType.Words
                    ? quoteFromApi.Split(' ')
                    : new[] { quoteFromApi };

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
                    if (!allowed)
                    {
                        continue;
                    }

                    if (configuration.ShouldContain.Count() > ShouldContainMinCount)
                    {
                        allowed = false;
                    }

                    if (!allowed)
                    {
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
