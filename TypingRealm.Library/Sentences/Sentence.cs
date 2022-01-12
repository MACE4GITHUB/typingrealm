using System.Collections.Generic;
using System.Diagnostics;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Sentences;

// Immutable aggregate root (with all public fields, record).
[DebuggerDisplay("{IndexInBook} - {Value}")]
public sealed record Sentence(
    BookId BookId,
    SentenceId SentenceId,
    int IndexInBook,
    string Value,
    IEnumerable<Word> Words);
