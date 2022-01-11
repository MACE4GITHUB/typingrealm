using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Library.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(Description))]
[Index(nameof(IsProcessed))]
[Index(nameof(IsArchived))]
[Index(nameof(AddedAtUtc))]
[Index(nameof(ContentId))]
[Index(nameof(Language))]
public class BookDao : IDao<BookDao>
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }

    [MaxLength(10)]
    public string Language { get; set; }

    [MaxLength(50)]
    public string ContentId { get; set; }
    public virtual BookContentDao Content { get; set; }

    public bool IsProcessed { get; set; }

    public bool IsArchived { get; set; }

    public DateTime AddedAtUtc { get; set; }

    public virtual ICollection<SentenceDao> Sentences { get; set; }

    public static BookDao ToDao(Book book)
    {
        var state = book.GetState();

        return new BookDao
        {
            Id = state.BookId,
            Description = state.Description,
            Language = state.Language,
            ContentId = state.BookId,
            IsProcessed = state.IsProcessed,
            IsArchived = state.IsArchived,
            AddedAtUtc = DateTime.UtcNow
        };
    }

    public Book FromDao()
    {
        var state = new Book.State(new(Id), Language, Description, IsProcessed, IsArchived);

        return Book.FromState(state);
    }

    public void MergeFrom(BookDao from)
    {
        if (Description != from.Description)
            Description = from.Description;

        if (Language != from.Language)
            Language = from.Language;

        if (ContentId != from.ContentId)
            ContentId = from.ContentId;

        if (IsProcessed != from.IsProcessed)
            IsProcessed = from.IsProcessed;

        if (IsArchived != from.IsArchived)
            IsArchived = from.IsArchived;

        if (AddedAtUtc == default && AddedAtUtc != from.AddedAtUtc)
            AddedAtUtc = from.AddedAtUtc;
    }
}
#pragma warning restore CS8618
