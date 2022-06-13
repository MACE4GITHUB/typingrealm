using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class BookQuery : IBookQuery
{
    private readonly LibraryDbContext _dbContext;

    public BookQuery(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<BookView>> FindAllBooksAsync()
    {
        var daos = await _dbContext.Book.AsNoTracking()
            .Where(x => !x.IsArchived)
            .ToListAsync()
            .ConfigureAwait(false);

        return daos
            .Select(x => DtoFromDao(x))
            .ToList();
    }

    public async ValueTask<BookView?> FindBookAsync(BookId bookId)
    {
        var dao = await _dbContext.Book.AsNoTracking()
            .SingleOrDefaultAsync(x => !x.IsArchived && x.Id == bookId.Value)
            .ConfigureAwait(false);

        if (dao == null)
            return null;

        return DtoFromDao(dao);
    }

    private static BookView DtoFromDao(BookDao dao)
    {
        return new BookView
        {
            BookId = dao.Id,
            Description = dao.Description,
            Language = dao.Language,
            ProcessingStatus = dao.ProcessingStatus,
            CreatedAtUtc = dao.CreatedAtUtc
        };
    }
}
