using System;
using System.Linq;

namespace TypingRealm.TextProcessing;

public sealed class LanguageInformation
{
    public LanguageInformation(Language language, string allowedCharacters)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(allowedCharacters);

        if (allowedCharacters.Length == 0)
            throw new ArgumentException("Allowed characters should have at least one character.");

        Language = language;
        AllowedCharacters = allowedCharacters;
    }

    public Language Language { get; }
    public string AllowedCharacters { get; }

    public bool IsAllLettersAllowed(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.All(character => AllowedCharacters.Contains(character));
    }
}
