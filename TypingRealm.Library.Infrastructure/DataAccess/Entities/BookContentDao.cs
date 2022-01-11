using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Library.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(BookId))]
public class BookContentDao : IDao<BookContentDao>
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; }

    public string Content { get; set; }

    [MaxLength(50)]
    public string BookId { get; set; }

    public static BookContentDao ToDao(string bookId, string content) => new BookContentDao
    {
        Id = bookId,
        Content = content,
        BookId = bookId
    };

    public BookContent FromDao() => new BookContent(
        new(BookId), new MemoryStream(Encoding.UTF8.GetBytes(Content))); // HACK.

    public void MergeFrom(BookContentDao from)
    {
        _ = from;

        throw new NotSupportedException("Cannot change book's content once created.");
    }
}
#pragma warning restore CS8618
