using System;
using System.Collections.Generic;
using TypingRealm.Texts.Retrievers;

namespace TypingRealm.Texts;

public static class SupportedLanguages
{
    public static Dictionary<string, Type> SupportedTextRetrievers => new()
    {
        //["en"] = typeof(QuotableEnglishTextRetriever),
        ["en"] = typeof(LibraryEnglishTextRetriever),
        ["ru"] = typeof(RussianTextRetriever)
    };

    public static IEnumerable<string> Languages => SupportedTextRetrievers.Keys;
}
