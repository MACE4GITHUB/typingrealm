using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Texts;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure.DataAccess.Entities
{
#pragma warning disable CS8618
    [Index(nameof(CreatedByUser))]
    [Index(nameof(CreatedUtc))]
    [Index(nameof(IsPublic))]
    [Index(nameof(IsArchived))]
    [Index(nameof(Language))]
    public class Text : IDbo<Text>
    {
        [Key]
        public string Id { get; set; }

        public string Value { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsPublic { get; set; }
        public bool IsArchived { get; set; }

        // Text configuration.
        public TextType TextType { get; set; }
        public int? GenerationLength { get; set; }
        public string? GenerationShouldContain { get; set; }
        public TextGenerationType? GenerationTextType { get; set; }

        [MaxLength(20)]
        public string Language { get; set; }

        public static Text ToDbo(Typing.Text.State state)
        {
            return new Text
            {
                Id = state.TextId,
                Value = state.Value,
                CreatedByUser = state.CreatedByUser,
                CreatedUtc = state.CreatedUtc,
                IsPublic = state.IsPublic,
                IsArchived = state.IsArchived,
                TextType = state.Configuration.TextType,
                GenerationLength = state.Configuration.TextGenerationConfiguration?.Length,
                GenerationShouldContain = state.Configuration.TextGenerationConfiguration?.ShouldContain == null
                    ? null : string.Join(',', state.Configuration.TextGenerationConfiguration.ShouldContain),
                GenerationTextType = state.Configuration.TextGenerationConfiguration?.GenerationTextType,
                Language = state.Configuration.Language
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
                IsArchived,
                new TextConfiguration(TextType, GenerationLength == null ? null : new Typing.TextGenerationConfiguration(
                    GenerationLength ?? throw new InvalidOperationException("GenerationLength is null in DB."),
                    GenerationShouldContain?.Split(',') ?? throw new InvalidOperationException("GenerationShouldContain is null in DB."),
                    GenerationTextType ?? throw new InvalidOperationException("GenerationTextType is null in DB.")),
                    Language));
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

            if (TextType != from.TextType)
                TextType = from.TextType;

            if (GenerationLength != from.GenerationLength)
                GenerationLength = from.GenerationLength;

            if (GenerationShouldContain != from.GenerationShouldContain)
                GenerationShouldContain = from.GenerationShouldContain;

            if (GenerationTextType != from.GenerationTextType)
                GenerationTextType = from.GenerationTextType;

            if (Language != from.Language)
                Language = from.Language;
        }
    }
#pragma warning restore CS8618
}
