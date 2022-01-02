using System;
using System.Net.Http;

namespace TypingRealm.Texts.Retrievers;

public sealed class RussianTextRetriever : HttpTextRetriever
{
    public RussianTextRetriever(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, "ru", new Uri("https://fish-text.ru/get?format=html"))
    {
    }

    protected override string AllowedLetters => "'\",<.>/?=+\\|-_;: 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";

    protected override string ResponseHandler(string response)
    {
        var value = response.Replace("<p>", string.Empty).Replace("</p>", string.Empty);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Text API returned empty text.");

        return value;
    }
}
