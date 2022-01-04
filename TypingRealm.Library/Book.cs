using System;

namespace TypingRealm.Library
{
    // Mutable aggregate root.
    public sealed class Book
    {
        public sealed record State(
            BookId BookId,
            string Description,
            string Content,
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

        public Book(BookId bookId, string description, string content)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                throw new ArgumentException("Book ID cannot be empty.", nameof(bookId));

            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(content);

            _state = new State(bookId, description, content, false, false);
        }

        public BookId BookId => _state.BookId;
        public string Content => _state.Content;

        public void Describe(string newDescription)
        {
            _state = _state with
            {
                Description = newDescription
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
}
