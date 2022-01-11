using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TypingRealm.Texts;

public static class TextHelpers
{
    // TODO: Unify this with TextRetrievers and all the other validation places.
    public static readonly string[] SupportedLanguages = new[] { "en", "ru" };

    // TODO: Consider moving this constant to Library context.
    public static readonly int MinSentenceLength = 8;

    // TODO: Append punctuation and numbers here from SupportedLanguages class.
    public static string AllowedEnglishLetters => "'\",<.>pPyYfFgGcCrRlL/?=+\\|aAoOeEuUiIdDhHtTnNsS-_;:qQjJkKxXbBmMwWvVzZ 1!2@3#4$5%6^7&8*9(0)[{]}`~";
    public static string AllowedRussianLetters => "'\",<.>/?=+\\|-_;: 1!2@3#4$5%6^7&8*9(0)[{]}`~йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";

    public static string PunctuationCharacters => "'\",<.>/?=+\\|-_;:!@#$%^&*()[{]}`~";
    public static string NumberCharacters => "0123456789";

    public static bool IsAllLettersAllowed(string text, string language)
    {
        return language switch
        {
            "en" => text.All(character => AllowedEnglishLetters.Contains(character)),
            "ru" => text.All(character => AllowedRussianLetters.Contains(character)),
            _ => throw new InvalidOperationException("Unknonw language."),
        };
    }

    private static readonly Regex _multipleSpacesRegex = new Regex(" {2,}", RegexOptions.Compiled);
    private static readonly Regex _sentenceRegex = new Regex(@"(?<=[\.!\?]""?)\s+", RegexOptions.Compiled);
    public static IEnumerable<string> GetSentencesEnumerable(string text)
    {
        return _sentenceRegex.Split(text)
            .Select(text =>
            {
                var sb = new StringBuilder(text);
                var normalizedText = sb.Replace("\r", string.Empty)
                    .Replace("\n", " ")
                    .Replace("“", "\"")
                    .Replace("”", "\"")
                    .Replace("—", " - ")
                    .Replace("–", "-")
                    .Replace("…\"", "...\" ")
                    .Replace("…", "... ")
                    .Replace("’", "'")
                    .Replace("‘", "'")
                    .Replace("†", " ")
                    .ToString().Trim();

                // Remove multiple spaces in a row.
                normalizedText = _multipleSpacesRegex.Replace(normalizedText, " ");

                var sentences = _sentenceRegex.Split(normalizedText)
                    .Where(sentence => sentence.Length > 0)
                    .Select(sentence =>
                    {
                        if (char.IsLower(sentence[0]))
                        {
                            if (sentence.Length == 1)
                                return sentence.ToUpperInvariant();

                            return $"{sentence[0].ToString().ToUpperInvariant()}{sentence[1..]}";
                        }

                        return sentence;
                    });

                return sentences;
            })
            .SelectMany(x => x)
            .Where(sentence => sentence.Length >= MinSentenceLength);
    }

    public static IEnumerable<string> GetAllowedSentencesEnumerable(string text, string language)
        => GetSentencesEnumerable(text).Where(sentence => IsAllLettersAllowed(sentence, language));

    public static string[] SplitTextBySpaces(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public static IEnumerable<string> GetWordsEnumerable(string text)
    {
        return GetSentencesEnumerable(text)
            .SelectMany(sentence => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static IEnumerable<string> GetAllowedWordsEnumerable(string text, string language)
        => GetWordsEnumerable(text).Where(word => IsAllLettersAllowed(word, language));

    /// <summary>
    /// Lowercase word without punctuation.
    /// </summary>
    public static string GetRawWord(string word)
    {
        return word.Trim(PunctuationCharacters.ToCharArray())
            .Trim()
            .ToLowerInvariant();
    }
}
