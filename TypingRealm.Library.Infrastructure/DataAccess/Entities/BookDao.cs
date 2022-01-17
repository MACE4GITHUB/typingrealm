using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(Description))]
[Index(nameof(ProcessingStatus))]
[Index(nameof(IsArchived))]
[Index(nameof(AddedAtUtc))]
[Index(nameof(ContentId))]
[Index(nameof(Language))]
public class BookDao : IDao<BookDao>
{
    [Key]
    [MaxLength(BookId.MaxLength)]
    public string Id { get; set; }

    [MaxLength(BookDescription.MaxLength)]
    public string Description { get; set; }

    [MaxLength(10)]
    public string Language { get; set; }

    [MaxLength(BookId.MaxLength)]
    public string ContentId { get; set; }
    public virtual BookContentDao Content { get; set; }

    public ProcessingStatus ProcessingStatus { get; set; }

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
            ProcessingStatus = state.ProcessingStatus,
            IsArchived = state.IsArchived,
            AddedAtUtc = DateTime.UtcNow
        };
    }

    public Book FromDao()
    {
        var state = new Book.State(new(Id), new(Language), new(Description), ProcessingStatus, IsArchived);

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

        if (ProcessingStatus != from.ProcessingStatus)
            ProcessingStatus = from.ProcessingStatus;

        if (IsArchived != from.IsArchived)
            IsArchived = from.IsArchived;

        if (AddedAtUtc == default && AddedAtUtc != from.AddedAtUtc)
            AddedAtUtc = from.AddedAtUtc;
    }
}
#pragma warning restore CS8618
