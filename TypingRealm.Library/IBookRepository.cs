using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface IBookRepository
{
    ValueTask<BookId> NextBookIdAsync();
    ValueTask<Book?> FindBookAsync(BookId bookId);
    ValueTask SaveBook(Book book);
}
