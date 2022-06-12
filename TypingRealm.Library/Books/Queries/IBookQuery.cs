using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library.Books.Queries;

public interface IBookQuery
{
    ValueTask<BookView?> FindBookAsync(BookId bookId);
    ValueTask<IEnumerable<BookView>> FindAllBooksAsync();
}
