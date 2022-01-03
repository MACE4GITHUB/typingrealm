using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
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
        ValueTask<string> GetNextTextValue(string language);
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

        public async ValueTask<string> GetNextTextValue(string language)
        {
            if (language == "en")
            {
                using var httpClient = _httpClientFactory.CreateClient();

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

            if (language == "ru")
            {
                using var httpClient = _httpClientFactory.CreateClient();

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

            throw new NotSupportedException($"Language {language} is not supported.");
        }
    }

    // TODO: Make sure there are no endless loops when asked for non-supported language.
    public sealed class RedisCachedTextRetriever : ITextRetriever
    {
        private const int CacheSize = 1000;
        private readonly ITextRetriever _textRetriever;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Dictionary<string, Task> _processes = new Dictionary<string, Task>();

        public RedisCachedTextRetriever(
            ITextRetriever textRetriever,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _textRetriever = textRetriever;
            _connectionMultiplexer = connectionMultiplexer;

            _processes.Add("en", StartGettingQuotesAsync("en"));
            _processes.Add("ru", StartGettingQuotesAsync("ru"));
        }

        public ValueTask<string> GetNextTextValue(string language) => GetQuoteAsync(language);

        private async ValueTask<string> GetQuoteAsync(string language)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var count = await db.SetLengthAsync(GetRedisKey(language))
                .ConfigureAwait(false);

            foreach (var process in _processes)
            {
                if (process.Value.IsCompleted && count < CacheSize)
                    _processes[process.Key] = StartGettingQuotesAsync(process.Key);
            }

            while (true)
            {
                var quote = await TryGetQuoteAsync(language)
                    .ConfigureAwait(false);

                if (quote != null)
                    return quote;

                await Task.Delay(10)
                    .ConfigureAwait(false);
            }
        }

        private async ValueTask<string?> TryGetQuoteAsync(string language)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var text = await db.SetPopAsync(GetRedisKey(language))
                .ConfigureAwait(false);

            if (!text.HasValue)
                return null;

            return text.ToString();
        }

        private async Task StartGettingQuotesAsync(string language)
        {
            var db = _connectionMultiplexer.GetDatabase();

            var iteration = 0;
            var count = await db.SetLengthAsync(GetRedisKey(language))
                .ConfigureAwait(false);

            while (count < CacheSize)
            {
                var text = await _textRetriever.GetNextTextValue(language)
                    .ConfigureAwait(false);

                await db.SetAddAsync(GetRedisKey(language), text)
                    .ConfigureAwait(false);

                iteration++;

                if (iteration > 50)
                {
                    count = await db.SetLengthAsync(GetRedisKey(language))
                        .ConfigureAwait(false);
                }
            }
        }

        private RedisKey GetRedisKey(string language)
        {
            return $"{language}_generated-texts";
        }
    }

    public sealed class InMemoryCachedTextRetriever : ITextRetriever
    {
        private const int CacheSize = 1000;
        private readonly ITextRetriever _textRetriever;
        private readonly Dictionary<string, ConcurrentQueue<string>> _quoteQueues = new Dictionary<string, ConcurrentQueue<string>>();
        private readonly Dictionary<string, Task> _processes = new Dictionary<string, Task>();

        public InMemoryCachedTextRetriever(ITextRetriever textRetriever)
        {
            _textRetriever = textRetriever;

            _processes.Add("en", StartGettingQuotesAsync("en"));
            _quoteQueues.Add("en", new ConcurrentQueue<string>());

            _processes.Add("ru", StartGettingQuotesAsync("ru"));
            _quoteQueues.Add("ru", new ConcurrentQueue<string>());
        }

        public ValueTask<string> GetNextTextValue(string language) => GetQuoteAsync(language);

        private async ValueTask<string> GetQuoteAsync(string language)
        {
            foreach (var process in _processes)
            {
                if (process.Value.IsCompleted && _quoteQueues[process.Key].Count < CacheSize)
                    _processes[process.Key] = StartGettingQuotesAsync(process.Key);
            }

            while (true)
            {
                var quote = await TryGetQuoteAsync(language)
                    .ConfigureAwait(false);

                if (quote != null)
                    return quote;

                await Task.Delay(10)
                    .ConfigureAwait(false);
            }
        }

        private async ValueTask<string?> TryGetQuoteAsync(string language)
        {
            if (_quoteQueues[language].TryDequeue(out var quote))
                return quote;

            return null;
        }

        private async Task StartGettingQuotesAsync(string language)
        {
            while (_quoteQueues[language].Count < CacheSize)
            {
                var text = await _textRetriever.GetNextTextValue(language)
                    .ConfigureAwait(false);

                _quoteQueues[language].Enqueue(text);
            }
        }
    }

    public sealed class TextGenerator : ITextGenerator
    {
        private const int MaxTextLength = 5000; // So that user cannot request 50000 characters.
        private const int ShouldContainMinCount = 15; // If there's not enough data - generate default text.

        private static readonly string _allowedLetters = "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";
        private readonly ITextRetriever _textRetriever;

        public TextGenerator(ITextRetriever textRetriever)
        {
            _textRetriever = textRetriever;
        }

        public async ValueTask<string> GenerateTextAsync(TextGenerationConfigurationDto configuration)
        {
            if (!IsSupported(configuration.Language))
                throw new NotSupportedException($"Language {configuration.Language} is not supported.");

            if (configuration.Length < 0)
                throw new InvalidOperationException("Cannot have negative length.");

            var minLength = configuration.Length == 0
                ? 10 // Default value.
                : configuration.Length;

            var builder = new StringBuilder();

            while (builder.Length < Math.Min(minLength, MaxTextLength))
            {
                var quoteFromApi = await _textRetriever.GetNextTextValue(configuration.Language)
                    .ConfigureAwait(false);

                var chunks = configuration.TextType == Texts.TextGenerationType.Words
                    ? quoteFromApi.Split('.').SelectMany(x => x.Split(' '))
                    : quoteFromApi.Split('.');

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

        private bool IsSupported(string language)
        {
            if (language == "en" || language == "ru")
                return true;

            return false;
        }
    }
}
