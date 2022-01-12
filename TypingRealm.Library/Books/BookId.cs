using System;
using TypingRealm.Common;

namespace TypingRealm.Library.Books;

public sealed class BookId : Identity
{
    public const int MaxLength = 50;

    public BookId(string value) : base(value)
    {
        Validation.ValidateLength(value, 1, MaxLength);
    }

    public static BookId New() => new(Guid.NewGuid().ToString());
}
