using System.Collections.Generic;

namespace TypingRealm.TextProcessing
{
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

        public static readonly string PunctuationCharacters = "'\",<.>/?=+\\|-_;:!@#$%^&*()[{]}`~";
        public static readonly char[] PunctuationCharactersArray = PunctuationCharacters.ToCharArray();
        public static readonly string NumberCharacters = "0123456789";
        public static readonly string SpaceCharacter = " ";
        public static Language EnglishLanguage => new(EnglishLanguageValue);
        public static Language RussianLanguage => new(RussianLanguageValue);
    }
}
