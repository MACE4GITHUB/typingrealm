using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Library.Api.Controllers;

public sealed record ImportBookRequest(string Description, IFormFile Content);

[Route(ServiceConfiguration.SentencesApiPrefix)]
public sealed class SentencesController : TyrController
{
    private readonly ISentenceQuery _sentenceQuery;

    public SentencesController(ISentenceQuery sentenceQuery)
    {
        _sentenceQuery = sentenceQuery;
    }

    [HttpGet]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentences(int count, int consecutiveCount)
    {
        var sentences = await _sentenceQuery.FindRandomSentencesAsync(count, consecutiveCount);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("keypairs")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByKeypairs(string[] keyPairs, int count)
    {
        var sentences = await _sentenceQuery.FindSentencesContainingKeyPairsAsync(keyPairs, count);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("words")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByWords(string[] words, int count)
    {
        var sentences = await _sentenceQuery.FindSentencesContainingWordsAsync(words, count);

        return Ok(sentences);
    }
}
