using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Sentences.Data;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Api.Sentences;

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
