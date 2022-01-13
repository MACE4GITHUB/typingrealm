using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;
using TypingRealm.Library.Importing;

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
        using var stream = content.OpenReadStream();

        return await _bookImporter.ImportNewBookAsync(description, language, stream);
    }

    [HttpPost]
    [SuperAdminScoped]
    [Route("{bookId}/re-import")]
    public async ValueTask<ActionResult<BookImportResult>> ReImportBook(string bookId)
    {
        return await _bookImporter.ReImportBookAsync(new(bookId));
    }
}
