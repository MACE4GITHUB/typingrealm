using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Books;
using TypingRealm.Library.Books.Queries;

namespace TypingRealm.Library.InMemoryInfrastructure;

public sealed class InMemoryBookRepository : IBookRepository, IBookQuery
{
    private readonly Dictionary<BookId, Book> _books = new Dictionary<BookId, Book>();
    private readonly Dictionary<BookId, BookContent> _bookContents
        = new Dictionary<BookId, BookContent>();

    public ValueTask AddBookWithContentAsync(Book book, BookContent content)
    {
        var state = book.GetState();
        if (_books.ContainsKey(state.BookId))
            throw new InvalidOperationException("Book already exists.");

        _books.Add(state.BookId, book);
        _bookContents.Add(content.BookId, content);

        return default;
    }

    public async ValueTask<IEnumerable<BookView>> FindAllBooksAsync()
    {
        return _books.Values.Select(b => b.GetState()).Select(b => new BookView
        {
            BookId = b.BookId,
            Language = b.Language,
            ProcessingStatus = b.ProcessingStatus,
            Description = b.Description,
            AddedAtUtc = default
        });
    }

    public ValueTask<Book?> FindBookAsync(BookId bookId)
    {
        if (_books.TryGetValue(bookId, out var book))
            return new(book);

        return new((Book?)null);
    }

    public async ValueTask<BookView?> FindBookAsync(string bookId)
    {
        return (await FindAllBooksAsync().ConfigureAwait(false))
            .FirstOrDefault(x => x.BookId == bookId);
    }

    public ValueTask<BookContent?> FindBookContentAsync(BookId bookId)
    {
        if (!_bookContents.ContainsKey(bookId))
            return new((BookContent?)null);

        return new(_bookContents[bookId]);
    }

    public ValueTask<BookId> NextBookIdAsync()
    {
        return new(BookId.New());
    }

    public ValueTask UpdateBookAsync(Book book)
    {
        var state = book.GetState();
        if (!_books.ContainsKey(state.BookId))
            throw new InvalidOperationException("Book does not exists.");

        _books[state.BookId] = book;

        return default;
    }
}
