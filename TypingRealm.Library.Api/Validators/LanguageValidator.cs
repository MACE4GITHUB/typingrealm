using System.Linq;
using FluentValidation;
using TypingRealm.Hosting;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Api.Validators;

public sealed class LanguageValidator : AbstractValidator<string>
{
    public LanguageValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Must(language => TextConstants.SupportedLanguageValues.Contains(language))
            .WithMessage("Language is not supported.")
            .MustCreate(x => new Language(x));
    }
}
