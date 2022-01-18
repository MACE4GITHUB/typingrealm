using System;
using System.Net.Http;
using TypingRealm.TextProcessing;

namespace TypingRealm.Texts.Retrievers;

public sealed class RussianTextRetriever : HttpTextRetriever
{
    public RussianTextRetriever(
        IHttpClientFactory httpClientFactory,
        ILanguageProvider languageProvider)
#pragma warning disable S1075 // URIs should not be hardcoded: this is a specific text retriever working with this exact URI and no other.
        : base(httpClientFactory, "ru", new Uri("https://fish-text.ru/get?format=html"))
#pragma warning restore S1075
    {
        var value = languageProvider.FindLanguageInformationAsync(new Language(Language));
        if (!value.IsCompletedSuccessfully)
            throw new NotSupportedException("Doesn't support async language provider.");

        AllowedLetters = value.Result.AllowedCharacters;
    }

    protected override string AllowedLetters { get; }

    protected override string ResponseHandler(string response)
    {
        var value = response.Replace("<p>", string.Empty).Replace("</p>", string.Empty);

        return value ?? throw new InvalidOperationException("Error when trying to get response from fishtext API: invalid content.");
    }
}
