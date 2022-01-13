using System;
using System.IO;

namespace TypingRealm.Library.Books;

/// <summary>
/// BookContent is a separate entity. It has the same identity as the Book to
/// which it is attached, and it has a data field that can be read for storing
/// or for Import process.
/// </summary>
public sealed record BookContent(BookId BookId, Stream Content);

/// <summary>
/// Book can be Imported into the Library. It contains Sentences and it can be
/// Re-Imported or Archived books are not used in queries.
/// IsProcessed field indicates whether the book Import process has been
/// finished successfully or no. In future we might rename it into IsImported.
/// </summary>
public sealed class Book
{
    public sealed record State(
        BookId BookId,
        Language Language,
        BookDescription Description,
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

    public Book(BookId bookId, Language language, BookDescription description)
    {
        ArgumentNullException.ThrowIfNull(bookId);
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(description);

        _state = new State(bookId, language, description, false, false);
    }

    public BookId BookId => _state.BookId;
    public string Language => _state.Language;
    public bool IsProcessed => _state.IsProcessed;

    public void Describe(BookDescription newDescription)
    {
        ArgumentNullException.ThrowIfNull(newDescription);

        _state = _state with
        {
            Description = newDescription
        };
    }

    public void StartReprocessing()
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
