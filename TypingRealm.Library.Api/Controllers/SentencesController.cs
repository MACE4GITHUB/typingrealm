using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Api.Controllers;

public sealed class LanguageQueryParameter
{
    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    [DefaultValue(TextConstants.DefaultLanguageValue)]
    public string Language { get; init; } = TextConstants.DefaultLanguageValue;
}

public sealed class LanguageQueryParameterValidator : AbstractValidator<LanguageQueryParameter>
{
    public LanguageQueryParameterValidator()
    {
        RuleFor(x => x.Language)
            .SetValidator(new LanguageValidator());
    }
}

public sealed class SentencesRequestValidator : AbstractValidator<SentencesRequest>
{
    public SentencesRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.IsValid())
            .WithMessage(x => string.Join("; ", x.GetErrorsIfInvalid()));
    }
}

public sealed class WordsRequestValidator : AbstractValidator<WordsRequest>
{
    public WordsRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.IsValid())
            .WithMessage(x => string.Join("; ", x.GetErrorsIfInvalid()));
    }
}

[Route(ServiceConfiguration.SentencesApiPrefix)]
public sealed class SentencesController : TyrController
{
    private readonly SentenceQueryResolver _sentenceQueryResolver;

    public SentencesController(SentenceQueryResolver sentenceQueryResolver)
    {
        _sentenceQueryResolver = sentenceQueryResolver;
    }

    /// <summary>
    /// Requests sentences data.
    /// </summary>
    /// <remarks>
    /// Gets a list of sentences based on the request.
    /// </remarks>
    [HttpPost]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.GetCollectionByQuery))]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetSentences(
        SentencesRequest request,
        [FromQuery] LanguageQueryParameter queryParameters)
    {
        var sentenceQuery = _sentenceQueryResolver(queryParameters.Language);

        var sentences = await sentenceQuery.FindSentencesAsync(request);

        return Ok(sentences);
    }

    /// <summary>
    /// Requests words data.
    /// </summary>
    /// <remarks>
    /// Gets a list of words based on the request.
    /// </remarks>
    [HttpPost]
    [Route("words")]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.GetCollectionByQuery))]
    public async ValueTask<ActionResult<IEnumerable<string>>> GetWords(
        WordsRequest request,
        [FromQuery] LanguageQueryParameter queryParameters)
    {
        var sentenceQuery = _sentenceQueryResolver(queryParameters.Language);

        var words = await sentenceQuery.FindWordsAsync(request);

        return Ok(words);
    }
}
