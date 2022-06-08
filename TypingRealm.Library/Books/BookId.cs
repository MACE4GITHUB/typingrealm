using System;

namespace TypingRealm.Library.Books;

public sealed class BookId : Identity
{
    public const int MinLength = 1;
    public const int MaxLength = 50;

    public BookId(string value) : base(value)
    {
        Validation.ValidateLength(value, MinLength, MaxLength);
    }

    public static BookId New() => new(Guid.NewGuid().ToString());
}
