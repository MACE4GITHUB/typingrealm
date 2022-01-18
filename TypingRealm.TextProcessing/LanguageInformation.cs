using System.Linq;

namespace TypingRealm.TextProcessing
{
    public sealed record LanguageInformation(
        Language Language,
        string AllowedCharacters)
    {
        public bool IsAllLettersAllowed(string text)
        {
            return text.All(character => AllowedCharacters.Contains(character));
        }
    }
}
