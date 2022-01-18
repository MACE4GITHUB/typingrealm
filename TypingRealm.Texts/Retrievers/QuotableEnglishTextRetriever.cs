﻿using System;
using System.Net.Http;
using System.Text.Json;
using TypingRealm.TextProcessing;

namespace TypingRealm.Texts.Retrievers;

public sealed class QuotableEnglishTextRetriever : HttpTextRetriever
{
    private sealed record QuotableResponse(string content);

    public QuotableEnglishTextRetriever(
        IHttpClientFactory httpClientFactory,
        ILanguageProvider languageProvider)
#pragma warning disable S1075 // URIs should not be hardcoded: this is a specific text retriever working with this exact URI and no other.
        : base(httpClientFactory, "en", new Uri("https://api.quotable.io/random"))
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
        var value = JsonSerializer.Deserialize<QuotableResponse>(response)?.content;

        return value ?? throw new InvalidOperationException("Error when trying to get response from quotable API: invalid content.");
    }
}
