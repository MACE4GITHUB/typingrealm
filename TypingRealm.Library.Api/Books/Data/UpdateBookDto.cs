namespace TypingRealm.Library.Api.Books.Data;

/// <summary>
/// Information to update in the existing book.
/// </summary>
public sealed record UpdateBookDto(string Description)
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/Description/*'/>
    public string Description { get; init; } = Description;
}
