using TypingRealm.Common;
using TypingRealm.Texts;

namespace TypingRealm.Library.Books;

public sealed class Language : Primitive<string>
{
    public Language(string value) : base(value)
    {
        Validation.ValidateIn(value, TextHelpers.SupportedLanguages);
    }
}
