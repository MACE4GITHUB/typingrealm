using System.Threading.Tasks;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Books;

public sealed class ArchiveBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ISentenceRepository _sentenceRepository;

    public ArchiveBookService(IBookRepository bookRepository, ISentenceRepository sentenceRepository)
    {
        _bookRepository = bookRepository;
        _sentenceRepository = sentenceRepository;
    }

    public async ValueTask ArchiveBookAsync(Book book)
    {
        book.Archive();

        await _bookRepository.UpdateBookAsync(book)
            .ConfigureAwait(false);

        await _sentenceRepository.RemoveAllForBook(book.BookId)
            .ConfigureAwait(false);
    }
}
