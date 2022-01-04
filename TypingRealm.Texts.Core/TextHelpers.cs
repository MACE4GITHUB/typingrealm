using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Texts;

public static class TextHelpers
{
    public static IEnumerable<string> GetSentencesEnumerable(string text)
    {
        return text.Split(". ", StringSplitOptions.RemoveEmptyEntries)
            .Select(text => text.TrimEnd('.'))
            .Select(text => $"{text}.");
    }

    public static IEnumerable<string> GetWordsEnumerable(string text)
    {
        return GetSentencesEnumerable(text)
            .SelectMany(sentence => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
