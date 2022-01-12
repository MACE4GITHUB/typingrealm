using System.Threading.Tasks;

namespace TypingRealm.Library.Books;

public interface IBookRepository
{
    ValueTask<BookId> NextBookIdAsync();
    ValueTask<Book?> FindBookAsync(BookId bookId);
    ValueTask<BookContent?> FindBookContentAsync(BookId bookId);
    ValueTask AddBookWithContentAsync(Book book, BookContent content);
    ValueTask UpdateBookAsync(Book book);
}
