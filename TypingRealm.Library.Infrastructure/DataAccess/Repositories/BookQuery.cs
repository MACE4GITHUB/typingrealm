using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Books.Queries;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class BookQuery : IBookQuery
{
    private readonly LibraryDbContext _dbContext;

    public BookQuery(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<BookDto>> FindAllBooksAsync()
    {
        var daos = await _dbContext.Book.AsNoTracking()
            .Where(x => !x.IsArchived)
            .ToListAsync()
            .ConfigureAwait(false);

        return daos
            .Select(x => new BookDto(x.Id, x.Language, x.Description, x.ProcessingStatus, x.AddedAtUtc))
            .ToList();
    }

    public async ValueTask<BookDto?> FindBookAsync(string bookId)
    {
        var dao = await _dbContext.Book.AsNoTracking()
            .SingleOrDefaultAsync(x => !x.IsArchived && x.Id == bookId)
            .ConfigureAwait(false);

        if (dao == null)
            return null;

        return new BookDto(dao.Id, dao.Language, dao.Description, dao.ProcessingStatus, dao.AddedAtUtc);
    }
}
