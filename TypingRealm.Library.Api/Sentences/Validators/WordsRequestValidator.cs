using FluentValidation;
using TypingRealm.Library.Sentences.Queries;

namespace TypingRealm.Library.Api.Sentences.Validators;

public sealed class WordsRequestValidator : AbstractValidator<WordsRequest>
{
    public WordsRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.IsValid())
            .WithMessage(x => string.Join("; ", x.GetErrorsIfInvalid()));
    }
}
