using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Generators;

public sealed class RussianTextRetriever : ITextRetriever
{
    private const int RetriesCount = 10;
    private static readonly string _allowedLetters = "'\",<.>/?=+\\|-_;: 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";
    private readonly IHttpClientFactory _httpClientFactory;

    public RussianTextRetriever(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string Language => "ru";

    public async ValueTask<string> RetrieveTextAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var count = 0; // Avoid endless loops.
        while (count < RetriesCount)
        {
            using var response = await httpClient.GetAsync("https://fish-text.ru/get?format=html")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var value = content.Replace("<p>", string.Empty).Replace("</p>", string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Text API returned empty text.");

            if (!value.All(character => _allowedLetters.Contains(character)))
                continue;

            return value;
        }

        throw new InvalidOperationException("Could not get text within allowed characters limit.");
    }
}
