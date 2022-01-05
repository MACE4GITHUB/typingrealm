using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;

namespace TypingRealm.Library.Api.Controllers;

public sealed record ImportBookRequest(string Description, IFormFile Content);

[Route(ServiceConfiguration.LibraryApiPrefix)]
public sealed class LibraryController : TyrController
{
    private readonly IBookImporter _bookImporter;
    private readonly ISentenceQuery _sentenceQuery;

    public LibraryController(
        IBookImporter bookImporter,
        ISentenceQuery sentenceQuery)
    {
        _bookImporter = bookImporter;
        _sentenceQuery = sentenceQuery;
    }

    [HttpGet]
    [Route("sentences")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentences(int count, int consecutiveCount)
    {
        var sentences = await _sentenceQuery.FindRandomSentencesAsync(count, consecutiveCount);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("sentences-by-keypairs")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByKeypairs(string[] keyPairs, int count)
    {
        var sentences = await _sentenceQuery.FindSentencesContainingKeyPairsAsync(keyPairs, count);

        return Ok(sentences);
    }

    [HttpGet]
    [Route("sentences-by-words")]
    public async ValueTask<ActionResult<IEnumerable<SentenceDto>>> GetRandomSentencesByWords(string[] words, int count)
    {
        var sentences = await _sentenceQuery.FindSentencesContainingWordsAsync(words, count);

        return Ok(sentences);
    }

    [HttpPost]
    [SuperAdminScoped]
    [Route("book/import")]
    public async ValueTask<ActionResult> ImportBook(string description, IFormFile content)
    {
        try
        {
            var textContent = await ReadAsStringAsync(content);
            await _bookImporter.ImportBookAsync(description, textContent);
        }
        catch (Exception exception)
        {
            return BadRequest($"Something bad happened during importing the book: {exception.Message}");
        }

        return Ok();
    }

    // TODO: Consider reading file part by part.
    public static async ValueTask<string> ReadAsStringAsync(IFormFile file)
    {
        var result = new StringBuilder();

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
                result.AppendLine(await reader.ReadLineAsync());
        }

        return result.ToString();
    }
}
