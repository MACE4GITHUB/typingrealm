using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.TextProcessing
{
    public interface ILanguageProvider
    {
        ValueTask<LanguageInformation> FindLanguageInformationAsync(Language language);
    }

    public sealed class LanguageProvider : ILanguageProvider
    {
        private const string AllowedEnglishLetters = "pPyYfFgGcCrRlLaAoOeEuUiIdDhHtTnNsSqQjJkKxXbBmMwWvVzZ";
        private const string AllowedRussianLetters = "йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮёЁ";

        private static readonly Dictionary<Language, string> _allowedCharacters = new()
        {
            [Constants.EnglishLanguage] = $"{AllowedEnglishLetters}{Constants.NumberCharacters}{Constants.PunctuationCharacters}",
            [Constants.RussianLanguage] = $"{AllowedRussianLetters}{Constants.NumberCharacters}{Constants.PunctuationCharacters}"
        };

        public ValueTask<LanguageInformation> FindLanguageInformationAsync(Language language)
        {
            if (!_allowedCharacters.ContainsKey(language))
                throw new NotSupportedException($"Language {language} is not supported.");

            return new(new LanguageInformation(language, _allowedCharacters[language]));
        }
    }
}
