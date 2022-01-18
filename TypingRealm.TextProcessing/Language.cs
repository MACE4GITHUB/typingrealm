using TypingRealm.Common;

namespace TypingRealm.TextProcessing
{
    public sealed class Language : Identity
    {
        public Language(string value) : base(value)
        {
            Validation.ValidateIn(value, Constants.SupportedLanguageValues);
        }
    }
}
