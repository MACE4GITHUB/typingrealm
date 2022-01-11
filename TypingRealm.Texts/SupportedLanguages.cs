using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Api.Client;
using TypingRealm.Texts.Retrievers;

namespace TypingRealm.Texts;

public static class SupportedLanguages
{
    public static Dictionary<string, KeyValuePair<Type, Func<IServiceProvider, ITextRetriever>?>> SupportedTextRetrievers => new()
    {
        //["en"] = typeof(QuotableEnglishTextRetriever),
        ["en"] = new(typeof(LibraryTextRetriever), provider => new LibraryTextRetriever(
            provider.GetRequiredService<ISentencesClient>(), "en")),

        ["ru"] = new(typeof(RussianTextRetriever), null)
    };

    public static IEnumerable<string> Languages => SupportedTextRetrievers.Keys;
}
