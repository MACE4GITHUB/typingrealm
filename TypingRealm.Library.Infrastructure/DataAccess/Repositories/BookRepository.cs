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

    public async ValueTask SaveBook(Book book)
    {
        var existing = await _dbContext.Book.FindAsync(book.BookId.Value)
            .ConfigureAwait(false);

        var dao = BookDao.ToDao(book);

        if (existing == null)
        {
            await _dbContext.Book.AddAsync(dao)
                .ConfigureAwait(false);
        }
        else
        {
            existing.MergeFrom(dao);
        }

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
