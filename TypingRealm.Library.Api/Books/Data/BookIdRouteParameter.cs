namespace TypingRealm.Library.Api.Books.Data;

// TODO: Consider declaring validators in the same file as DTO object.
// But think how it can be done if we have SDK kind of project for DTOs.
public sealed record BookIdRouteParameter(string BookId)
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/BookId/*'/>
    public string BookId { get; init; } = BookId;
}
