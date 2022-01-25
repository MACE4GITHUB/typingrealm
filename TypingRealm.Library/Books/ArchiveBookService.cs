using System;
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

    public async ValueTask ArchiveBookAsync(BookId bookId)
    {
        var book = await _bookRepository.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException("Book doesn't exist.");

        book.Archive();

        await _bookRepository.UpdateBookAsync(book)
            .ConfigureAwait(false);

        await _sentenceRepository.RemoveAllForBook(bookId)
            .ConfigureAwait(false);
    }
}
