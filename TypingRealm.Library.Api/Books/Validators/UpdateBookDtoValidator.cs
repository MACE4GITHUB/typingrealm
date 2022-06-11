using FluentValidation;
using TypingRealm.Library.Api.Books.Data;

namespace TypingRealm.Library.Api.Books.Validators;

public sealed class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookDtoValidator()
    {
        RuleFor(x => x.Description)
            .SetValidator(new BookDescriptionValidator());
    }
}
