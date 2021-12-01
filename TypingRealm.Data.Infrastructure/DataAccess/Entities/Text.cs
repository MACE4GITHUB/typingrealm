using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Data.Infrastructure.DataAccess.Entities
{
#pragma warning disable CS8618
    [Index(nameof(CreatedByUser))]
    [Index(nameof(CreatedUtc))]
    [Index(nameof(IsPublic))]
    [Index(nameof(IsArchived))]
    public class Text : IDbo<Text>
    {
        [Key]
        public string Id { get; set; }

        public string Value { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsPublic { get; set; }
        public bool IsArchived { get; set; }

        public static Text ToDbo(Typing.Text.State state)
        {
            return new Text
            {
                Id = state.TextId,
                Value = state.Value,
                CreatedByUser = state.CreatedByUser,
                CreatedUtc = state.CreatedUtc,
                IsPublic = state.IsPublic,
                IsArchived = state.IsArchived
            };
        }

        public Typing.Text.State ToState()
        {
            return new Typing.Text.State(
                Id,
                Value,
                CreatedByUser,
                CreatedUtc,
                IsPublic,
                IsArchived);
        }

        public void MergeFrom(Text from)
        {
            // TODO: Consider throwing an exception if field that shouldn't change are changed.
            if (Value != from.Value)
                Value = from.Value;

            if (CreatedByUser != from.CreatedByUser)
                CreatedByUser = from.CreatedByUser;

            if (CreatedUtc != from.CreatedUtc)
                CreatedUtc = from.CreatedUtc;

            if (IsPublic != from.IsPublic)
                IsPublic = from.IsPublic;

            if (IsArchived != from.IsArchived)
                IsArchived = from.IsArchived;
        }
    }
#pragma warning restore CS8618
}
