using FluentValidation;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Api.Sentences.Validators;

public sealed class SentencesRequestValidator : AbstractValidator<SentencesRequest>
{
    public SentencesRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.IsValid())
            .WithMessage(x => string.Join("; ", x.GetErrorsIfInvalid()));
    }
}
