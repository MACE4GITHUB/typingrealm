using System.Collections.Generic;

namespace TypingRealm.Library;

// Immutable aggregate root (with all public fields, record).
public sealed record Sentence(
    BookId BookId,
    SentenceId SentenceId,
    int Index,
    string Value,
    IEnumerable<Word> Words);
