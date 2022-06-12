namespace TypingRealm.Library.Api.Books.Data;

/// <summary>
/// Response after uploading the Book.
/// </summary>
public sealed record UploadBookResponse(string BookId)
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/BookId/*'/>
    public string BookId { get; init; } = BookId;
}
