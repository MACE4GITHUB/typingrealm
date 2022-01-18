using System.Collections.Generic;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Importing;

public sealed record BookImportResult(
    Book book,
    IEnumerable<string> TooShortSentences,
    IEnumerable<string> NotAllowedSentences,
    string NotAllowedCharacters);
