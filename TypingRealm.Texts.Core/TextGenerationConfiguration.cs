using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Texts;

public sealed record TextGenerationConfiguration(
    string Language,
    int Length,
    TextStructure Structure,
    bool IsLowerCase,
    bool StripPunctuation,
    bool StripNumbers,
    IEnumerable<string> ShouldContain)
{
    private static readonly int[] _allowedStandardTextLengths = new[] { 100, 300, 500, 700, 900 };

    public TextGenerationMode Mode
    {
        get
        {
            if ((!_allowedStandardTextLengths.Contains(Length))
                || IsLowerCase
                || StripPunctuation)
                return TextGenerationMode.Custom;

            if (Structure == TextStructure.Text)
            {
                if (ShouldContain.Any())
                    return TextGenerationMode.Custom;

                return TextGenerationMode.StandardText;
            }

            if (Structure == TextStructure.Words)
                return TextGenerationMode.StandardWords;

            return TextGenerationMode.Custom;
        }
    }

    public static TextGenerationConfiguration StandardText(string language, int length)
    {
        if (!_allowedStandardTextLengths.Contains(length))
            throw new ArgumentException("Length is invalid, use one of predefined values.", nameof(length));

        return new TextGenerationConfiguration(
            language,
            length,
            TextStructure.Text,
            false, false, false,
            Enumerable.Empty<string>());
    }

    public static TextGenerationConfiguration StandardWords(string language, int length, IEnumerable<string> shouldContain)
    {
        if (!_allowedStandardTextLengths.Contains(length))
            throw new ArgumentException("Length is invalid, use one of predefined values.", nameof(length));

        return new TextGenerationConfiguration(
            language,
            length,
            TextStructure.Words,
            false, false, false,
            shouldContain);
    }
}
