namespace TypingRealm.Library.Books.Queries;

/// <summary>
/// Book information without context that can be Queried through the API.
/// </summary>
public sealed class BookView : TrackableView
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/BookId/*'/>
    public string BookId { get; init; } = null!;

    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    public string Language { get; init; } = null!;

    /// <include file='../ApiDocs.xml' path='Api/Library/Book/Description/*'/>
    public string Description { get; init; } = null!;

    /// <include file='../ApiDocs.xml' path='Api/Library/Book/ProcessingStatus/*'/>
    public ProcessingStatus ProcessingStatus { get; init; }
}
