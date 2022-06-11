using FluentValidation;
using TypingRealm.Library.Api.Books.Data;
using TypingRealm.Library.Api.Validators;

namespace TypingRealm.Library.Api.Books.Validators;

public sealed class UploadBookDtoValidator : AbstractValidator<UploadBookDto>
{
    public UploadBookDtoValidator()
    {
        RuleFor(x => x.Description)
            .SetValidator(new BookDescriptionValidator());

        RuleFor(x => x.Language)
            .SetValidator(new LanguageValidator());

        RuleFor(x => x.Content)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Content.Length)
            .ExclusiveBetween(10, 20_000_000);

        /*// Need to dispose of stream & make sure this works to use this validation.
        RuleFor(x => x.Content)
            .MustCreate(x => new BookContent(BookId.New(), x.OpenReadStream()));*/
    }
}
