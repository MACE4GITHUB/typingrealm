using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Api.Controllers;

public sealed record ImportBookRequest(string Description, IFormFile Content);

[Route(ServiceConfiguration.SentencesApiPrefix)]
public sealed class SentencesController : TyrController
{
    private readonly SentenceQueryResolver _sentenceQueryResolver;

    public SentencesController(SentenceQueryResolver sentenceQueryResolver)
    {
        _sentenceQueryResolver = sentenceQueryResolver;
    }

    [HttpGet]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentences(string language, int count, int consecutiveCount)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var sentences = await sentenceQuery.FindRandomSentencesAsync(count, consecutiveCount);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("keypairs")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByKeypairs(string language, string[] keyPairs, int count)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var sentences = await sentenceQuery.FindSentencesContainingKeyPairsAsync(keyPairs, count);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("words")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByWords(string language, string[] words, int count)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var sentences = await sentenceQuery.FindSentencesContainingWordsAsync(words, count);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("pure-words")]
    public async ValueTask<ActionResult<IEnumerable<string>>> FindWordsContainingKeyPairs(string language, string[] keyPairs, int maxWordsCount, bool rawWords)
    {
        var sentenceQuery = _sentenceQueryResolver(language);

        var words = await sentenceQuery.FindWordsContainingKeyPairsAsync(keyPairs, maxWordsCount, rawWords);

        return Ok(words);
    }
}
