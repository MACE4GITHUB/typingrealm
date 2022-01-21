using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TypingRealm.TextProcessing
{
    public interface ITextProcessor
    {
        IEnumerable<string> GetSentencesEnumerable(string text);
        IEnumerable<string> GetWordsEnumerable(string text);
        string NormalizeWord(string word);
    }

    public sealed class TextProcessor : ITextProcessor
    {
        private static readonly Regex _sentenceRegex = new Regex(@"(?<=[\.!\?]""?)\s+", RegexOptions.Compiled);
        private static readonly Regex _multipleSpacesRegex = new Regex(" {2,}", RegexOptions.Compiled);
        private static readonly char[] _wordTrimCharacters = $"{TextConstants.PunctuationCharacters} ".ToCharArray();

        public IEnumerable<string> GetSentencesEnumerable(string text)
        {
            return _sentenceRegex.Split(text)
                .Select(sentence => NormalizeCharacters(sentence))
                .SelectMany(sentence => _sentenceRegex.Split(sentence))
                .Where(sentence => sentence.Length > 0)
                .Select(sentence => CapitalizeFirstCharacter(sentence));
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
                .Replace("\t", " ")
                .Replace("«", "\"")
                .Replace("»", "\"")
                .Replace(" ", " ") // These are custom unicode characters.
                .Replace("‚", ",")
                .ToString().Trim();

            // Remove multiple spaces in a row.
            normalizedText = _multipleSpacesRegex.Replace(normalizedText, " ");

            return normalizedText;
        }

        private static string CapitalizeFirstCharacter(string text)
        {
            if (char.IsLower(text[0]))
            {
                if (text.Length == 1)
                    return text.ToUpperInvariant();

                return $"{text[0].ToString().ToUpperInvariant()}{text[1..]}";
            }

            return text;
        }
    }
}
