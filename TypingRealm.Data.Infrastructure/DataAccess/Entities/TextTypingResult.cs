using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Data.Infrastructure.DataAccess.Entities
{
#pragma warning disable CS8618
    [Index(nameof(TypingSessionTextIndex))]
    [Index(nameof(StartedTypingUtc))]
    [Index(nameof(SubmittedResultsUtc))]
    [Index(nameof(UserSessionId))]
    public class TextTypingResult
    {
        [Key]
        public string Id { get; set; }

        public int TypingSessionTextIndex { get; set; }
        public DateTime StartedTypingUtc { get; set; }
        public DateTime SubmittedResultsUtc { get; set; }
        public virtual ICollection<KeyPressEvent> Events { get; set; }

        public string UserSessionId { get; set; }
        public UserSession UserSession { get; set; }

        public static TextTypingResult ToDbo(Typing.TextTypingResult state, string userSessionId)
        {
            return new TextTypingResult
            {
                Id = state.TextTypingResultId,
                TypingSessionTextIndex = state.TypingSessionTextIndex,
                StartedTypingUtc = state.StartedTypingUtc,
                SubmittedResultsUtc = state.SubmittedResultsUtc,
                Events = state.Events
                    .Select(e => KeyPressEvent.ToDbo(e, state.TextTypingResultId))
                    .ToList(),
                UserSessionId = userSessionId
            };
        }

        public Typing.TextTypingResult ToState()
        {
            return new Typing.TextTypingResult(
                Id,
                TypingSessionTextIndex,
                StartedTypingUtc,
                SubmittedResultsUtc,
                Events.Select(e => e.ToState())
                    .ToList());
        }

        public void MergeFrom(Typing.TextTypingResult from)
        {
            _ = from;
            throw new InvalidOperationException("TextTypingResult entity is immutable.");
        }
    }
#pragma warning restore CS8618
}
