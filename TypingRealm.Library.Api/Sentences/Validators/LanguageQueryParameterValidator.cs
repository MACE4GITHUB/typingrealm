using FluentValidation;
using TypingRealm.Library.Api.Sentences.Data;
using TypingRealm.Library.Api.Validators;

namespace TypingRealm.Library.Api.Sentences.Validators;

public sealed class LanguageQueryParameterValidator : AbstractValidator<LanguageQueryParameter>
{
    public LanguageQueryParameterValidator()
    {
        RuleFor(x => x.Language)
            .SetValidator(new LanguageValidator());
    }
}
