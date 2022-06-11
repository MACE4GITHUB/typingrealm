using Microsoft.AspNetCore.Http;

namespace TypingRealm.Library.Api.Books.Data;

/// <summary>
/// Information to upload to the Library as the new book.
/// </summary>
public sealed class UploadBookDto
{
    /// <include file='../ApiDocs.xml' path='Api/Library/Book/Description/*'/>
    public string Description { get; init; } = null!;

    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    public string Language { get; init; } = null!;

    /// <summary>
    /// Content of the file. It should be a text file with text (UTF-8) content.
    /// </summary>
    public IFormFile Content { get; init; } = null!;
}
