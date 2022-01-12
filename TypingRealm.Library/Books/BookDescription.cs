using TypingRealm.Common;

namespace TypingRealm.Library.Books;

public sealed class BookDescription : Primitive<string>
{
    public const int MaxLength = 100;

    public BookDescription(string value) : base(value)
    {
        Validation.ValidateLength(value, 1, MaxLength);
    }
}
