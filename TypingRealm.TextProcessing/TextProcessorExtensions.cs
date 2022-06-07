using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.TextProcessing;

public static class TextProcessorExtensions
{
    public static IEnumerable<string> GetSentencesEnumerable(
        this ITextProcessor textProcessor,
        string text,
        LanguageInformation languageInformation)
    {
        return textProcessor.GetSentencesEnumerable(text)
            .Where(sentence => languageInformation.IsAllLettersAllowed(sentence));
    }

    // HACK method for Library to get statistics.
    public static IEnumerable<string> GetNotAllowedSentencesEnumerable(
        this ITextProcessor textProcessor,
        string text,
        LanguageInformation languageInformation)
    {
        return textProcessor.GetSentencesEnumerable(text)
            .Where(sentence => !languageInformation.IsAllLettersAllowed(sentence));
    }

    public static IEnumerable<string> GetWordsEnumerable(
        this ITextProcessor textProcessor,
        string text,
        LanguageInformation languageInformation)
    {
        return textProcessor.GetWordsEnumerable(text)
            .Where(word => languageInformation.IsAllLettersAllowed(word));
    }

    public static IEnumerable<string> GetNormalizedWordsEnumerable(
        this ITextProcessor textProcessor, string text)
    {
        return textProcessor.GetWordsEnumerable(text)
            .Select(word => textProcessor.NormalizeWord(word))
            .Where(word => word.Length > 0)
            .Distinct(); // Normalized words are distinct, just GetWordsEnumerable are consecutive all.
    }

    public static IEnumerable<string> GetNormalizedWordsEnumerable(
        this ITextProcessor textProcessor,
        string text,
        LanguageInformation languageInformation)
    {
        return textProcessor.GetNormalizedWordsEnumerable(text)
            .Where(word => languageInformation.IsAllLettersAllowed(word));
    }
}
