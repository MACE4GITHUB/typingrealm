using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Sentences;
using TypingRealm.Texts;

namespace TypingRealm.Library.Api.Controllers;

[Route(ServiceConfiguration.SentencesApiPrefix)]
public sealed class SentencesController : TyrController
{
    private readonly SentenceQueryResolver _sentenceQueryResolver;

    public SentencesController(SentenceQueryResolver sentenceQueryResolver)
    {
        _sentenceQueryResolver = sentenceQueryResolver;
    }

    [HttpPost]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetSentences(
        SentencesRequest request, string language = TextHelpers.DefaultLanguage)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var sentences = await sentenceQuery.FindSentencesAsync(request);

        return Ok(sentences);
    }

    [HttpPost]
    [Route("words")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetWords(
        WordsRequest request, string language = TextHelpers.DefaultLanguage)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var words = await sentenceQuery.FindWordsAsync(request);

        return Ok(words);
    }
}
