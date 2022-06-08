namespace TypingRealm.Library.Books;

public sealed class BookDescription : Primitive<string>
{
    public const int MaxLength = 100;
    public const int MinLength = 1;

    public BookDescription(string value) : base(value)
    {
        Validation.ValidateLength(value, MinLength, MaxLength);
    }
}
