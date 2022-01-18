using System.Collections.Generic;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Importing;

public sealed record BookImportResult(
    Book book,
    IEnumerable<string> Top10NotAllowedSentences,
    string NotAllowedCharacters,
    bool IsSuccessful);
