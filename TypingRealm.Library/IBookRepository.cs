using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface IBookRepository
{
    ValueTask<BookId> NextBookIdAsync();

    ValueTask<Book?> FindBookAsync(BookId bookId);
    ValueTask<BookContent?> FindBookContent(BookId bookContentId);

    ValueTask AddBookWithContent(Book book, BookContent content);
    ValueTask UpdateBook(Book book);
}
