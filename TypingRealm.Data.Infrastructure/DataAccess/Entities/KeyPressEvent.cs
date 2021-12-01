using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure.DataAccess.Entities
{
#pragma warning disable CS8618
    [Index(nameof(TextTypingResultId))]
    public class KeyPressEvent
    {
        [Key]
        public long Id { get; set; }

        public int Index { get; set; }
        public KeyAction KeyAction { get; set; }
        public string Key { get; set; }
        public decimal AbsoluteDelay { get; set; }

        public string TextTypingResultId { get; set; }
        public TextTypingResult TextTypingResult { get; set; }

        public static KeyPressEvent ToDbo(Typing.KeyPressEvent state, string textTypingResultId)
        {
            return new KeyPressEvent
            {
                Id = 0,
                Index = state.Index,
                KeyAction = state.KeyAction,
                Key = state.Key,
                AbsoluteDelay = state.AbsoluteDelay,
                TextTypingResultId = textTypingResultId
            };
        }

        public Typing.KeyPressEvent ToState()
        {
            return new Typing.KeyPressEvent(
                Index,
                KeyAction,
                Key,
                AbsoluteDelay);
        }

        public void MergeFrom(Typing.KeyPressEvent from)
        {
            _ = from;
            throw new InvalidOperationException("KeyPressEvent entity is immutable.");
        }
    }
#pragma warning restore CS8618
}
