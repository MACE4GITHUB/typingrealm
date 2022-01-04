using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Library.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(IsProcessed))]
[Index(nameof(IsArchived))]
public class BookDao : IDao<BookDao>
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; }

    public string Description { get; set; }

    public string Content { get; set; }

    public bool IsProcessed { get; set; }

    public bool IsArchived { get; set; }

    public static BookDao ToDao(Book book)
    {
        var state = book.GetState();

        return new BookDao
        {
            Id = state.BookId,
            Description = state.Description,
            Content = state.Content,
            IsProcessed = state.IsProcessed,
            IsArchived = state.IsArchived
        };
    }

    public Book FromDao()
    {
        var state = new Book.State(new(Id), Description, Content, IsProcessed, IsArchived);

        return Book.FromState(state);
    }

    public void MergeFrom(BookDao from)
    {
        if (Description != from.Description)
            Description = from.Description;

        if (Content != from.Content)
            Content = from.Content;

        if (IsProcessed != from.IsProcessed)
            IsProcessed = from.IsProcessed;

        if (IsArchived != from.IsArchived)
            IsArchived = from.IsArchived;
    }
}
#pragma warning restore CS8618
