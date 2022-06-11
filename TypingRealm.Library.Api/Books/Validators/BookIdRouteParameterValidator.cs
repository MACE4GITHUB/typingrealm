using FluentValidation;
using TypingRealm.Library.Api.Books.Data;

namespace TypingRealm.Library.Api.Books.Validators;

public sealed class BookIdRouteParameterValidator : AbstractValidator<BookIdRouteParameter>
{
    public BookIdRouteParameterValidator()
    {
        RuleFor(x => x.BookId)
            .SetValidator(new BookIdValidator());
    }
}
