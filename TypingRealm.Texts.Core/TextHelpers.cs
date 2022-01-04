using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Texts;

public static class TextHelpers
{
    // TODO: Consider moving this constant to Library context.
    public static int MinSentenceLength = 8;

    // TODO: Append punctuation and numbers here from SupportedLanguages class.
    public static string AllowedEnglishLetters => "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
    public static string AllowedRussianLetters => "'\",<.>/?=+\\|-_;: 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";

    public static bool IsAllLettersAllowed(string text, string language)
    {
        return language switch
        {
            "en" => text.All(character => AllowedEnglishLetters.Contains(character)),
            "ru" => text.All(character => AllowedRussianLetters.Contains(character)),
            _ => throw new InvalidOperationException("Unknonw language."),
        };
    }

    public static IEnumerable<string> GetSentencesEnumerable(string text)
    {
        return text.Split(". ", StringSplitOptions.RemoveEmptyEntries)
            .Select(text => text.Replace("\r", ""))
            .Select(text => text.Replace("\n", " "))
            .Select(text => text.TrimEnd('.'))
            .Select(text => string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries))) // Remove multiple spaces in a row.
            .Select(text => $"{text}.");
    }

    public static IEnumerable<string> GetWordsEnumerable(string text)
    {
        return GetSentencesEnumerable(text)
            .SelectMany(sentence => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
