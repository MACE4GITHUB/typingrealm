using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TypingRealm.TextProcessing;

public interface ITextProcessor
{
    IEnumerable<string> GetSentencesEnumerable(string text);
    IEnumerable<string> GetWordsEnumerable(string text);
    string NormalizeWord(string word);
}

public sealed class TextProcessor : ITextProcessor
{
    private static readonly Regex _sentenceRegex = new(@$"(?<=[\.!\?][{TextConstants.PunctuationCharactersForRegex}]*)\s+", RegexOptions.Compiled);
    private static readonly Regex _multipleSpacesRegex = new(" {2,}", RegexOptions.Compiled);
    private static readonly char[] _wordTrimCharacters = $"{TextConstants.PunctuationCharacters} ".ToCharArray();

    public IEnumerable<string> GetSentencesEnumerable(string text)
    {
        return _sentenceRegex.Split(text)
            .Select(sentence => NormalizeCharacters(sentence))
            .SelectMany(sentence => _sentenceRegex.Split(sentence))
            .Where(sentence => sentence.Length > 0)
            .Where(sentence => !sentence.All(character => ".!? ".Contains(character)))
            .Select(sentence => CapitalizeFirstLetter(sentence));
    }

    public IEnumerable<string> GetWordsEnumerable(string text)
    {
        return GetSentencesEnumerable(text)
            .SelectMany(sentence => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

#pragma warning disable CA1308 // Normalize strings to uppercase: We need them exactly in lowercase.
    public string NormalizeWord(string word)
    {
        return word.Trim(_wordTrimCharacters)
            .ToLowerInvariant();
    }
#pragma warning restore CA1308

    private static string NormalizeCharacters(string text)
    {
        var sb = new StringBuilder(text);
        var normalizedText = sb
            .Replace("\r", string.Empty)
            .Replace("\n", " ")
            .Replace("“", "\"")
            .Replace("”", "\"")
            .Replace("«", "\"")
            .Replace("»", "\"")
            .Replace("’", "'")
            .Replace("‘", "'")
            .Replace("‚", ",")
            .Replace("\t", " ")
            .Replace(" ", " ") // These are custom unicode characters.
            .Replace("†", " ")
            .Replace("–", "-")
            .Replace("—", " - ")
            .Replace("…", "...")
            .ToString().Trim();

        if (normalizedText.StartsWith("-"))
            normalizedText = $"- {normalizedText[1..]}";

        // Remove multiple spaces in a row.
        normalizedText = _multipleSpacesRegex.Replace(normalizedText, " ")
            .Trim();

        if (normalizedText.Length == 0)
            return normalizedText;

        // TODO: Add a dot at the end here if last character is not punctuation.

        // TODO: Optimize this to not copy strings.
        if (normalizedText.EndsWith(" .", StringComparison.Ordinal))
            normalizedText = $"{normalizedText[0..^2]}.";
        if (normalizedText.EndsWith(" ?", StringComparison.Ordinal))
            normalizedText = $"{normalizedText[0..^2]}?";
        if (normalizedText.EndsWith(" !", StringComparison.Ordinal))
            normalizedText = $"{normalizedText[0..^2]}!";

        if (!normalizedText.EndsWith(".", StringComparison.Ordinal)
            && !normalizedText.EndsWith("!", StringComparison.Ordinal)
            && !normalizedText.EndsWith("?", StringComparison.Ordinal)
            && !normalizedText.EndsWith("\"", StringComparison.Ordinal))
            normalizedText = $"{normalizedText}.";

        return normalizedText;
    }

    private static string CapitalizeFirstLetter(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (TextConstants.PunctuationCharacters.Contains(text[i]) || text[i] == TextConstants.SpaceCharacter)
                continue;

            if (char.IsLower(text[i]))
            {
                if (text.Length == 1)
                    return text.ToUpperInvariant();

                var firstPart = i > 0 ? text[..i] : string.Empty;
                var secondPart = text.Length > i ? text[(i + 1)..] : string.Empty;
                return $"{firstPart}{text[i].ToString().ToUpperInvariant()}{secondPart}";
            }

            return text;
        }

        return text;
    }
}
