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
    /// Retrieves text from third-party service. Can be cached.
    /// </summary>
    public interface ITextRetriever
    {
        /// <summary>
        /// Gets arbitrary text value from third-party service.
        /// </summary>
        /// <returns></returns>
        ValueTask<string> GetNextTextValue();
    }

    public sealed class QuotableTextRetriever : ITextRetriever
    {
#pragma warning disable CS8618
        private sealed class QuotableResponse
        {
            public string content { get; set; }
        }
#pragma warning restore CS8618

        private readonly IHttpClientFactory _httpClientFactory;

        public QuotableTextRetriever(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> GetNextTextValue()
        {
            var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.GetAsync("http://api.quotable.io/random")
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

    public sealed class InMemoryCachedTextRetriever : ITextRetriever
    {
        private const int CacheSize = 1000;
        private readonly ITextRetriever _textRetriever;
        private readonly ConcurrentQueue<string> _quoteQueue = new ConcurrentQueue<string>();
        private Task _process;

        public InMemoryCachedTextRetriever(ITextRetriever textRetriever)
        {
            _textRetriever = textRetriever;
            _process = StartGettingQuotesAsync();
        }

        public ValueTask<string> GetNextTextValue() => GetQuoteAsync();

        private async ValueTask<string> GetQuoteAsync()
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

        private async ValueTask<string?> TryGetQuoteAsync()
        {
            if (_quoteQueue.TryDequeue(out var quote))
                return quote;

            return null;
        }

        private async Task StartGettingQuotesAsync()
        {
            while (_quoteQueue.Count < CacheSize)
            {
                var text = await _textRetriever.GetNextTextValue()
                    .ConfigureAwait(false);

                _quoteQueue.Enqueue(text);
            }
        }
    }

    public sealed class TextGenerator : ITextGenerator
    {
        private const int MaxTextLength = 5000; // So that user cannot request 50000 characters.
        private const int ShouldContainMinCount = 15; // If there's not enough data - generate default text.

        private static readonly string _allowedLetters = "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
        private readonly ITextRetriever _textRetriever;

        public TextGenerator(ITextRetriever textRetriever)
        {
            _textRetriever = textRetriever;
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
                var quoteFromApi = await _textRetriever.GetNextTextValue()
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
