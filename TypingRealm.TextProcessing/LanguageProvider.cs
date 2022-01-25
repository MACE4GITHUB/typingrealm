using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.TextProcessing;

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
        [TextConstants.EnglishLanguage] = $"{AllowedEnglishLetters}{TextConstants.NumberCharacters}{TextConstants.PunctuationCharacters}{TextConstants.SpaceCharacter}",
        [TextConstants.RussianLanguage] = $"{AllowedRussianLetters}{TextConstants.NumberCharacters}{TextConstants.PunctuationCharacters}{TextConstants.SpaceCharacter}"
    };

    public ValueTask<LanguageInformation> FindLanguageInformationAsync(Language language)
    {
        if (!_allowedCharacters.ContainsKey(language))
            throw new NotSupportedException($"Language {language} is not supported.");

        return new(new LanguageInformation(language, _allowedCharacters[language]));
    }
}
