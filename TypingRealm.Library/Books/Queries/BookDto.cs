using System;

namespace TypingRealm.Library.Books.Queries
{
    public sealed record BookDto(
        string BookId,
        string Language,
        string Description,
        ProcessingStatus ProcessingStatus,
        DateTime AddedAtUtc);
}
