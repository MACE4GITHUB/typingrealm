using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Hosting;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Importing;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Api.Controllers;

public sealed class BookIdRouteParameter
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/BookId/*'/>
    public string BookId { get; init; } = null!;
}

/// <summary>
/// Information to update in the existing book.
/// </summary>
public sealed class UpdateBookDto
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/Description/*'/>
    public string Description { get; init; } = null!;
}

/// <summary>
/// Information to upload to the Library as the new book.
/// </summary>
public sealed class UploadBookDto
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/Description/*'/>
    public string Description { get; init; } = null!;

    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    public string Language { get; init; } = null!;

    /// <summary>
    /// Content of the file. It should be a text file with text (UTF-8) content.
    /// </summary>
    public IFormFile Content { get; init; } = null!;
}

/// <summary>
/// Response after uploading the Book.
/// </summary>
public sealed class UploadBookResponse
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/BookId/*'/>
    public string BookId { get; init; } = null!;
}

public sealed class BookIdValidator : AbstractValidator<string>
{
    public BookIdValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Length(BookId.MinLength, BookId.MaxLength)
            .MustCreate(x => new BookId(x));
    }
}

public sealed class BookDescriptionValidator : AbstractValidator<string>
{
    public BookDescriptionValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Length(BookDescription.MinLength, BookDescription.MaxLength)
            .MustCreate(x => new BookDescription(x));
    }
}

public sealed class LanguageValidator : AbstractValidator<string>
{
    public LanguageValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .Must(language => TextConstants.SupportedLanguageValues.Contains(language))
            .WithMessage("Language is not supported.")
            .MustCreate(x => new Language(x));
    }
}

public sealed class BookIdRouteParameterValidator : AbstractValidator<BookIdRouteParameter>
{
    public BookIdRouteParameterValidator()
    {
        RuleFor(x => x.BookId)
            .SetValidator(new BookIdValidator());
    }
}

public sealed class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookDtoValidator()
    {
        RuleFor(x => x.Description)
            .SetValidator(new BookDescriptionValidator());
    }
}

public sealed class UploadBookDtoValidator : AbstractValidator<UploadBookDto>
{
    public UploadBookDtoValidator()
    {
        RuleFor(x => x.Description)
            .SetValidator(new BookDescriptionValidator());

        RuleFor(x => x.Language)
            .SetValidator(new LanguageValidator());
    }
}

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

    /// <summary>
    /// Gets all books without their content.
    /// </summary>
    /// <remarks>
    /// Gets a collection of all Books added to the system, returns just the
    /// information about them, without the content. Archived books won't show up.
    /// When no books are present in the library - returns an empty list [].
    ///
    /// Sample request:
    ///
    ///     GET /api/books
    /// </remarks>
    [HttpGet]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.GetAll))]
    public async ValueTask<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
    {
        var books = await _bookQuery.FindAllBooksAsync();

        return Ok(books);
    }

    /// <summary>
    /// Finds a single book and returns the information without the content.
    /// </summary>
    /// <remarks>
    /// Searches and returns a single book that has the specified identifier.
    /// If no book is found with such id, 404 (NotFound) result is returned.
    ///
    /// Sample request:
    ///
    ///     GET /api/books/123myBookId
    /// </remarks>
    [HttpGet("{bookId}")]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.GetSingle))]
    public async ValueTask<ActionResult<BookDto>> GetBookById(
        [FromQuery] BookIdRouteParameter bookIdRouteParameter)
    {
        var book = await _bookQuery.FindBookAsync(bookIdRouteParameter.BookId);
        if (book == null)
            return NotFound();

        return Ok(book);
    }

    /// <summary>
    /// Updates book information.
    /// </summary>
    /// <param name="dto">Book information for update, these fields will be updated in the existing book.</param>
    /// <remarks>
    /// Updates the book, overwriting specified parameters on the existing Book.
    /// If no book is found - returns 404 (NotFound) result.
    ///
    /// Sample request:
    ///
    ///     PUT /api/books/123myBookId
    ///     {
    ///         "description": "My new book description"
    ///     }
    /// </remarks>
    [HttpPut("{bookId}")]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.BusinessActionNoContent))]
    public async ValueTask<IActionResult> UpdateBook(
        [FromRoute] BookIdRouteParameter bookIdRouteParameter,
        UpdateBookDto dto)
    {
        var book = await _bookRepository.FindBookAsync(new(bookIdRouteParameter.BookId));
        if (book == null)
            return NotFound();

        book.Describe(new(dto.Description));

        await _bookRepository.UpdateBookAsync(book);
        return NoContent();
    }

    /// <summary>
    /// Archives the Book.
    /// </summary>
    /// <remarks>
    /// This is soft delete. This marks the Book as deleted and removes all
    /// sentences data. There is no functionality to restore the Book for now.
    /// If we have it in future - you would need to re-import the Book after
    /// restoring it.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/books/{bookId}
    /// </remarks>
    [HttpDelete("{bookId}")]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.BusinessActionNoContent))]
    public async ValueTask<IActionResult> ArchiveBook(
        [FromRoute] BookIdRouteParameter bookIdRouteParameter)
    {
        var book = await _bookRepository.FindBookAsync(new(bookIdRouteParameter.BookId))
            .ConfigureAwait(false);

        if (book == null)
            return NotFound();

        await _archiveBookService.ArchiveBookAsync(book);
        return NoContent();
    }

    /// <summary>
    /// Uploads a new Book, but does not import it.
    /// </summary>
    /// <remarks>
    /// Uploading the Book does NOT import it, it just adds its content and
    /// description to the Library. No new sentences data will appear in the
    /// application until you Import this Book with `/api/books/{bookId}/import`
    /// endpoint.
    ///
    /// Sample request:
    ///
    ///     POST /books?description=SomeDescription&amp;language=en
    ///
    /// `content` should be form-data file.
    /// </remarks>
    [HttpPost]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.BusinessActionWithContent))]
    public async ValueTask<ActionResult<UploadBookResponse>> UploadBook(
        [FromQuery] UploadBookDto dto)
    {
        using var stream = dto.Content.OpenReadStream();

        var bookId = await _bookRepository.NextBookIdAsync();
        var book = new Book(bookId, new Language(dto.Language), new BookDescription(dto.Description));
        var bookContent = new BookContent(bookId, stream);

        await _bookRepository.AddBookWithContentAsync(book, bookContent);

        var result = new UploadBookResponse
        {
            BookId = bookId
        };

        return CreatedAtAction(nameof(GetBookById), result, result);
    }

    /// <summary>
    /// Imports a book that exists in the Library.
    /// </summary>
    /// <remarks>
    /// Parses the content of the book, splits it into sentences, words and
    /// keypairs and adds it to the sentences data. The book cannot be imported
    /// twice.
    ///
    /// Sample request:
    ///
    ///     POST /books/123myBookId/import
    /// </remarks>
    [HttpPost("{bookId}/import")]
    [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.BusinessActionWithContent))]
    public async ValueTask<ActionResult<BookImportResult>> ImportBook(
        [FromRoute] BookIdRouteParameter bookIdRouteParameter)
    {
        var book = await _bookRepository.FindBookAsync(new(bookIdRouteParameter.BookId))
            .ConfigureAwait(false);

        if (book == null)
            return NotFound();

        return await _bookImporter.ImportBookAsync(book);
    }
}
