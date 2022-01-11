using System;
using System.IO;
using System.Linq;
using TypingRealm.Texts;

namespace TypingRealm.Library;

public sealed record BookContent(BookId BookId, Stream Content);

// Mutable aggregate root.
public sealed class Book
{
    public sealed record State(
        BookId BookId,
        string Language,
        string Description,
        bool IsProcessed,
        bool IsArchived);

    #region State
    private State _state;
    private Book(State state)
    {
        _state = state with { };
    }
    public static Book FromState(State state) => new Book(state);
    public State GetState() => _state with { };
    #endregion

    public Book(BookId bookId, string language, string description)
    {
        if (string.IsNullOrWhiteSpace(bookId))
            throw new ArgumentException("Book ID cannot be empty.", nameof(bookId));

        ArgumentNullException.ThrowIfNull(description);

        if (!TextHelpers.SupportedLanguages.Contains(language))
            throw new ArgumentException($"Language {language} is not supported.");

        _state = new State(bookId, language, description, false, false);
    }

    public BookId BookId => _state.BookId;
    public string Language => _state.Language;

    public void Describe(string newDescription)
    {
        _state = _state with
        {
            Description = newDescription
        };
    }

    public void Reprocess()
    {
        if (_state.IsArchived)
            throw new InvalidOperationException("Book has already been archived.");

        if (!_state.IsProcessed)
            throw new InvalidOperationException("Book has not been processed yet.");

        _state = _state with
        {
            IsProcessed = false
        };
    }

    public void FinishProcessing()
    {
        if (_state.IsArchived)
            throw new InvalidOperationException("Book has already been archived.");

        if (_state.IsProcessed)
            throw new InvalidOperationException("This book has already been processed.");

        _state = _state with
        {
            IsProcessed = true
        };
    }

    public void Archive()
    {
        if (_state.IsArchived)
            throw new InvalidOperationException("Book has already been archived.");

        _state = _state with
        {
            IsArchived = true
        };
    }
}
