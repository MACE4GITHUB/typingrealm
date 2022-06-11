using FluentValidation;
using TypingRealm.Hosting;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Api.Books.Validators;

public sealed class BookDescriptionValidator : AbstractValidator<string>
{
    public BookDescriptionValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Length(BookDescription.MinLength, BookDescription.MaxLength)
            .MustCreate(x => new BookDescription(x));
    }
}
