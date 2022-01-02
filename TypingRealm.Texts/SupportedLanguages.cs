using System;
using System.Collections.Generic;
using TypingRealm.Texts.Retrievers;

namespace TypingRealm.Texts;

public static class SupportedLanguages
{
    public static IDictionary<string, Type> SupportedTextRetrievers
        => new Dictionary<string, Type>()
        {
            ["en"] = typeof(EnglishTextRetriever),
            ["ru"] = typeof(RussianTextRetriever)
        };
}
