using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library.Books.Queries;

public interface IBookQuery
{
    ValueTask<BookDto?> FindBookAsync(string bookId);
    ValueTask<IEnumerable<BookDto>> FindAllBooksAsync();
}
