using System.Collections.Generic;

namespace TypingRealm.TextProcessing;

// This file is not unit tested and is critical to contain valid consistent data
// as it is being reused in multiple domains.
public static class TextConstants
{
    public const string DefaultLanguageValue = EnglishLanguageValue;
    private const string EnglishLanguageValue = "en";
    private const string RussianLanguageValue = "ru";
    internal static IEnumerable<string> SupportedLanguageValues => new[]
    {
            EnglishLanguageValue,
            RussianLanguageValue
        };

    public static readonly string PunctuationCharacters = "'\",<.>/?=+\\|_;:!@#$%^&*()[{]}`~-";
    public static string PunctuationCharactersForRegex => PunctuationCharacters
        .Replace(@"\", @"\\")
        .Replace("-", @"\-")
        .Replace("[", @"\[")
        .Replace("]", @"\]")
        .Replace(@"^", @"\^");

    public static readonly char[] PunctuationCharactersArray = PunctuationCharacters.ToCharArray();
    public static readonly string NumberCharacters = "0123456789";
    public static readonly char SpaceCharacter = ' ';
    public static Language EnglishLanguage => new(EnglishLanguageValue);
    public static Language RussianLanguage => new(RussianLanguageValue);
}
