using System.Collections.Generic;

namespace TypingRealm.Library;

public sealed record BookImportResult(
    Book book,
    IEnumerable<string> Top10NotAllowedSentences,
    string NotAllowedCharacters);
