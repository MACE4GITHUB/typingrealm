using System;
using System.IO;
using System.Threading.Tasks;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _dbContext;

    public BookRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Book?> FindBookAsync(BookId bookId)
    {
        var dao = await _dbContext.Book.FindAsync(bookId.Value)
            .ConfigureAwait(false);

        return dao?.FromDao();
    }

    public ValueTask<BookId> NextBookIdAsync()
    {
        return new(BookId.New());
    }

    public async ValueTask AddBookWithContent(Book book, BookContent content)
    {
        var existing = await _dbContext.Book.FindAsync(book.BookId.Value)
            .ConfigureAwait(false);

        if (existing != null)
            throw new InvalidOperationException("The book has already been imported (added).");

        var dao = BookDao.ToDao(book);

        var transaction = await _dbContext.Database.BeginTransactionAsync()
            .ConfigureAwait(false);

        if (existing != null)
            throw new InvalidOperationException("Book already exists.");

        // TODO: Read by chunks.
        using var reader = new StreamReader(content.Content);
        var stringContent = await reader.ReadToEndAsync()
            .ConfigureAwait(false);

        await _dbContext.BookContent.AddAsync(BookContentDao.ToDao(content.BookId, stringContent))
            .ConfigureAwait(false);

        await _dbContext.Book.AddAsync(dao)
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);

        await transaction.CommitAsync()
            .ConfigureAwait(false);
    }

    public async ValueTask<BookContent?> FindBookContent(BookId bookId)
    {
        var dao = await _dbContext.BookContent.FindAsync(bookId.Value)
            .ConfigureAwait(false);

        return dao?.FromDao();
    }

    public async ValueTask UpdateBook(Book book)
    {
        var existing = await _dbContext.Book.FindAsync(book.BookId.Value)
            .ConfigureAwait(false);

        if (existing == null)
            throw new InvalidOperationException("The book doesn't exist.");

        var dao = BookDao.ToDao(book);
        existing.MergeFrom(dao);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
