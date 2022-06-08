using System.ComponentModel.DataAnnotations;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Api.Data;

public sealed record UpdateBookDto(
    [StringLength(BookDescription.MaxLength, MinimumLength = BookDescription.MinLength)]
    string Description);
