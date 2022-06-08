using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Data;
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
    private readonly IBookRepository _bookRepository;
    private readonly ArchiveBookService _archiveBookService;

    public BooksController(
        IBookImporter bookImporter,
        IBookQuery bookQuery,
        IBookRepository bookRepository,
        ArchiveBookService archiveBookService)
    {
        _bookImporter = bookImporter;
        _bookQuery = bookQuery;
        _bookRepository = bookRepository;
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
        if (book == null)
            throw new NotFoundApiException("message");
            //return NotFound();

        return Ok(book);
    }

    [HttpPut]
    [Route("{bookId}")]
    public async ValueTask<ActionResult> UpdateBook(
        [StringLength(BookId.MaxLength, MinimumLength = BookId.MinLength)] string bookId,
        UpdateBookDto dto)
    {
        var book = await _bookRepository.FindBookAsync(new(bookId));
        if (book == null)
            return NotFound();

        book.Describe(new(dto.Description));

        await _bookRepository.UpdateBookAsync(book);
        return Ok();
    }

    [HttpDelete]
    [Route("{bookId}")]
    public async ValueTask<ActionResult> ArchiveBook(string bookId)
    {
        await _archiveBookService.ArchiveBookAsync(new(bookId));
        return Ok();
    }

    [HttpPost]
    public async ValueTask<ActionResult> UploadBook(string description, string language, IFormFile content)
    {
        using var stream = content.OpenReadStream();

        var bookId = await _bookRepository.NextBookIdAsync();
        var book = new Book(bookId, new(language), new(description));
        var bookContent = new BookContent(bookId, stream);

        await _bookRepository.AddBookWithContentAsync(book, bookContent);

        var result = new { bookId = bookId.Value };
        return CreatedAtAction(nameof(GetBookById), result, result);
    }

    [HttpPost]
    [Route("{bookId}/import")]
    public async ValueTask<ActionResult<BookImportResult>> ImportBook(string bookId)
    {
        return await _bookImporter.ImportBookAsync(new(bookId));
    }
}
