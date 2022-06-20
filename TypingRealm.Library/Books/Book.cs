using System;
using System.IO;
using System.Linq.Expressions;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Books;

/// <summary>
/// BookContent is a separate entity. It has the same identity as the Book to
/// which it is attached, and it has a data field that can be read for storing
/// or for Import process.
/// </summary>
public sealed record BookContent(BookId BookId, Stream Content);

/// <include file='../ApiDocs.xml' path='Api/Library/Book/ProcessingStatus/*'/>
public enum ProcessingStatus
{
    NotProcessed = 1,
    Processing,
    Processed,
    Error
}

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
        ProcessingStatus ProcessingStatus,
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

        _state = new State(bookId, language, description, ProcessingStatus.NotProcessed, false);
    }

    internal BookId BookId => _state.BookId;
    internal Language Language => _state.Language;

    public void Describe(BookDescription newDescription)
    {
        ArgumentNullException.ThrowIfNull(newDescription);

        _state = _state with
        {
            Description = newDescription
        };
    }

    public void StartProcessing()
    {
        if (_state.IsArchived)
            throw new DomainException("Book has already been archived.");

        _state = _state with
        {
            ProcessingStatus = ProcessingStatus.Processing
        };
    }

    public void FinishProcessing()
    {
        if (_state.IsArchived)
            throw new DomainException("Book has already been archived.");

        if (_state.ProcessingStatus != ProcessingStatus.Processing)
            throw new DomainException("This book is not started processing yet.");

        _state = _state with
        {
            ProcessingStatus = ProcessingStatus.Processed
        };
    }

    public void ErrorProcessing()
    {
        if (_state.IsArchived)
            throw new DomainException("Book has already been archived.");

        if (_state.ProcessingStatus != ProcessingStatus.Processing)
            throw new DomainException("This book is not started processing yet.");

        _state = _state with
        {
            ProcessingStatus = ProcessingStatus.Error
        };
    }

    public void Archive()
    {
        var canBeArchived = new CanBeArchivedSpecification();
        if (!canBeArchived.IsSatisfiedBy(_state))
            throw new DomainException("Book cannot be archived in current state. It has probably already been archived.");

        _state = _state with
        {
            IsArchived = true,
            ProcessingStatus = ProcessingStatus.NotProcessed
        };
    }
}

public sealed class CanBeArchivedSpecification : Specification<Book.State>
{
    public override Expression<Func<Book.State, bool>> ToExpression()
    {
        return state => !state.IsArchived
            && state.ProcessingStatus != ProcessingStatus.Processing;
    }
}
