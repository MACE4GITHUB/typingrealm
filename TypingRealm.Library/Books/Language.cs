using TypingRealm.Common;
using TypingRealm.Texts;

namespace TypingRealm.Library.Books;

public sealed class Language : Primitive<string>
{
    public Language(string value) : base(value)
    {
        // TODO: Unify SupportedLanguages between all contexts.
        // TODO: Unit test this validation after unifying SupportedLanguages within all these contexts.
        Validation.ValidateIn(value, TextHelpers.SupportedLanguages);
    }
}
