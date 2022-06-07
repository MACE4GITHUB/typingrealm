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
        if (text.Length == 0)
            return string.Empty;

        var sb = new StringBuilder(text)
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
            .Replace("…", "...");

        // Trim string builder.
        while (sb.Length > 0 && sb[0] == ' ')
            sb.Remove(0, 1);

        while (sb.Length > 0 && sb[^1] == ' ')
            sb.Length--;

        if (sb.Length == 0)
            return string.Empty; // This will happen only if text contains only \r symbols. TODO: Unit test this.

        if (sb[0] == '-')
            sb.Insert(1, ' ');

        // Need to remove multiple spaces here, before checking endings.
        var index = 0;
        var previousSpace = false;
        while (true)
        {
            if (sb[index] == ' ')
            {
                if (previousSpace)
                {
                    sb.Remove(index, 1);
                    continue;
                }

                previousSpace = true;
            }
            else
            {
                previousSpace = false;
            }

            index++;
            if (index >= sb.Length)
                break;
        }

        if (sb.Length >= 2 && sb[^2] == ' ' && (sb[^1] == '.' || sb[^1] == '?' || sb[^1] == '!'))
            sb.Remove(sb.Length - 2, 1);

        if (sb.Length == 0)
            return string.Empty;

        var lastCharacter = sb[^1];
        if (lastCharacter != '.' && lastCharacter != '!' && lastCharacter != '?' && lastCharacter != '\"')
            sb.Append('.');

        return sb.ToString();
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
