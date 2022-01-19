using System.Linq;

namespace TypingRealm.TextProcessing
{
    public sealed record LanguageInformation(
        Language Language,
        string AllowedCharacters)
    {
        public bool IsAllLettersAllowed(string text)
        {
            // TODO: Validate this during construction.
            if (AllowedCharacters == null || text == null)
                return false;

            return text.All(character => AllowedCharacters.Contains(character));
        }
    }
}
