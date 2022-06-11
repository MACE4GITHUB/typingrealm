using FluentValidation;
using TypingRealm.Hosting;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Api.Books.Validators;

public sealed class BookIdValidator : AbstractValidator<string>
{
    public BookIdValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Length(BookId.MinLength, BookId.MaxLength)
            .MustCreate(x => new BookId(x));
    }
}
