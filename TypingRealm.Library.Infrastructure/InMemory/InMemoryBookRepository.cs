using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Infrastructure.InMemory;

public sealed class InMemoryBookRepository : IBookRepository
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

    public ValueTask<Book?> FindBookAsync(BookId bookId)
    {
        if (_books.TryGetValue(bookId, out var book))
            return new(book);

        return new((Book?)null);
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
