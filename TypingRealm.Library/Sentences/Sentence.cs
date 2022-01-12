using System.Collections.Generic;
using System.Diagnostics;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Sentences;

[DebuggerDisplay("{IndexInBook} - {Value}")]
public sealed record Sentence(
    BookId BookId,
    SentenceId SentenceId,
    int IndexInBook,
    string Value,
    IEnumerable<Word> Words);
