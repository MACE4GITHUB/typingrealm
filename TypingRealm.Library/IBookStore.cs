using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface IBookStore
{
    ValueTask<BookId> NextBookIdAsync();
    ValueTask<Book?> FindBookAsync(BookId bookId);
    ValueTask SaveBook(Book book);
}
