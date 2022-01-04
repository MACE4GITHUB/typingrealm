using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Library.Api.Controllers;

public sealed record ImportBookRequest(string Description, string Content);

[Route(ServiceConfiguration.LibraryApiPrefix)]
public sealed class LibraryController : TyrController
{
    private readonly IBookImporter _bookImporter;

    public LibraryController(IBookImporter bookImporter)
    {
        _bookImporter = bookImporter;
    }

    [HttpPost]
    [Route("book/import")]
    public async ValueTask<ActionResult> ImportBook(ImportBookRequest request)
    {
        try
        {
            await _bookImporter.ImportBookAsync(request.Description, request.Content);
        }
        catch (Exception exception)
        {
            return BadRequest($"Something bad happened during importing the book: {exception.Message}");
        }

        return Ok();
    }
}
