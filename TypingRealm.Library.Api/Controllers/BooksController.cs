using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;

namespace TypingRealm.Library.Api.Controllers;

[Route(ServiceConfiguration.BooksApiPrefix)]
public sealed class BooksController : TyrController
{
    private readonly IBookImporter _bookImporter;

    public BooksController(IBookImporter bookImporter)
    {
        _bookImporter = bookImporter;
    }

    [HttpPost]
    [SuperAdminScoped]
    [Route("import")]
    public async ValueTask<ActionResult<BookImportResult>> ImportBook(string description, string language, IFormFile content)
    {
        try
        {
            var textContent = await ReadAsStringAsync(content);

            using var stream = content.OpenReadStream();

            return await _bookImporter.ImportBookAsync(description, language, stream);
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
