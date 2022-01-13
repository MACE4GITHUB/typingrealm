using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Importing;

namespace TypingRealm.Library.Api.Controllers;

[Route(ServiceConfiguration.BooksApiPrefix)]
[SuperAdminScoped]
public sealed class BooksController : TyrController
{
    private readonly IBookImporter _bookImporter;
    private readonly IBookQuery _bookQuery;
    private readonly ArchiveBookService _archiveBookService;

    public BooksController(
        IBookImporter bookImporter,
        IBookQuery bookQuery,
        ArchiveBookService archiveBookService)
    {
        _bookImporter = bookImporter;
        _bookQuery = bookQuery;
        _archiveBookService = archiveBookService;
    }

    [HttpGet]
    public async ValueTask<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
    {
        var books = await _bookQuery.FindAllBooksAsync();

        return Ok(books);
    }

    [HttpGet]
    [Route("{bookId}")]
    public async ValueTask<ActionResult<BookDto>> GetBookById(string bookId)
    {
        var book = await _bookQuery.FindBookAsync(bookId);

        return Ok(book);
    }

    [HttpDelete]
    [Route("{bookId}")]
    public async ValueTask<ActionResult> ArchiveBook(string bookId)
    {
        await _archiveBookService.ArchiveBookAsync(new(bookId));
        return Ok();
    }

    [HttpPost]
    [Route("import")]
    public async ValueTask<ActionResult<BookImportResult>> ImportBook(string description, string language, IFormFile content)
    {
        using var stream = content.OpenReadStream();

        return await _bookImporter.ImportNewBookAsync(description, language, stream);
    }

    [HttpPost]
    [Route("{bookId}/re-import")]
    public async ValueTask<ActionResult<BookImportResult>> ReImportBook(string bookId)
    {
        return await _bookImporter.ReImportBookAsync(new(bookId));
    }
}
