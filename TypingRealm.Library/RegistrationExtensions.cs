using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Library;

public static class RegistrationExtensions
{
    public static IServiceCollection AddLibraryDomain(this IServiceCollection services)
    {
        return services
            .AddSingleton<IBookRepository, InMemoryBookStore>()
            .AddSingleton<ISentenceRepository, InMemorySentenceRepository>()
            .AddTransient<IBookImporter, BookImporter>();
    }
}

public sealed class InMemoryBookStore : IBookRepository
{
    private readonly Dictionary<BookId, Book> _books = new Dictionary<BookId, Book>();
    private readonly Dictionary<BookId, BookContent> _bookContents
        = new Dictionary<BookId, BookContent>();

    public ValueTask AddBookWithContent(Book book, BookContent content)
    {
        if (_books.ContainsKey(book.BookId))
            throw new InvalidOperationException("Book already exists.");

        _books.Add(book.BookId, book);
        _bookContents.Add(content.BookId, content);

        return default;
    }

    public ValueTask<Book?> FindBookAsync(BookId bookId)
    {
        if (_books.TryGetValue(bookId, out var book))
            return new(book);

        return new((Book?)null);
    }

    public ValueTask<BookContent?> FindBookContent(BookId bookId)
    {
        if (!_bookContents.ContainsKey(bookId))
            return new((BookContent?)null);

        return new(_bookContents[bookId]);
    }

    public ValueTask<BookId> NextBookIdAsync()
    {
        return new(BookId.New());
    }

    public ValueTask UpdateBook(Book book)
    {
        if (!_books.ContainsKey(book.BookId))
            throw new InvalidOperationException("Book does not exists.");

        _books[book.BookId] = book;

        return default;
    }
}

public sealed class InMemorySentenceRepository : ISentenceRepository
{
    private readonly Dictionary<SentenceId, Sentence> _sentences
        = new Dictionary<SentenceId, Sentence>();

    public ValueTask<SentenceId> NextIdAsync()
    {
        return new(SentenceId.New());
    }

    public ValueTask SaveAsync(Sentence sentence)
    {
        if (!_sentences.ContainsKey(sentence.SentenceId))
            _sentences.Add(sentence.SentenceId, sentence);
        else
            _sentences[sentence.SentenceId] = sentence;

        return default;
    }

    public async ValueTask SaveByBatchesAsync(IEnumerable<Sentence> allSentences, int batchSize)
    {
        foreach (var sentence in allSentences)
        {
            await SaveAsync(sentence)
                .ConfigureAwait(false);
        }
    }
}
