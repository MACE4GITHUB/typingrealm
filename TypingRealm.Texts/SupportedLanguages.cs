using System;
using System.Collections.Generic;
using TypingRealm.Texts.Retrievers;

namespace TypingRealm.Texts;

public static class SupportedLanguages
{
    public static Dictionary<string, Type> SupportedTextRetrievers => new()
    {
        ["en"] = typeof(EnglishTextRetriever),
        ["ru"] = typeof(RussianTextRetriever)
    };

    public static IEnumerable<string> Languages => SupportedTextRetrievers.Keys;
}
