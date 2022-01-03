using System;
using System.Net.Http;

namespace TypingRealm.Texts.Retrievers;

public sealed class RussianTextRetriever : HttpTextRetriever
{
    public RussianTextRetriever(IHttpClientFactory httpClientFactory)
#pragma warning disable S1075 // URIs should not be hardcoded: this is a specific text retriever working with this exact URI and no other.
        : base(httpClientFactory, "ru", new Uri("https://fish-text.ru/get?format=html"))
#pragma warning restore S1075
    {
    }

    protected override string AllowedLetters => "'\",<.>/?=+\\|-_;: 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";

    protected override string ResponseHandler(string response)
    {
        var value = response.Replace("<p>", string.Empty).Replace("</p>", string.Empty);

        return value ?? throw new InvalidOperationException("Error when trying to get response from fishtext API: invalid content.");
    }
}
