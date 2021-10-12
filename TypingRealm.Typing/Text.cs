using System;
using System.Collections.Generic;

namespace TypingRealm.Typing
{
    public sealed class Text
    {
        public Text(
            Guid textId,
            string value,
            decimal totalTimeMs,
            DateTimeOffset startedTypingAt,
            DateTime submittedAtUtc,
            IEnumerable<KeyPressEvent> events)
        {
            TextId = textId;
            Value = value;
            TotalTimeMs = totalTimeMs;
            StartedTypingAt = startedTypingAt;
            SubmittedAtUtc = submittedAtUtc;
            Events = events;
        }

        public Guid TextId { get; }
        public string Value { get; }
        public decimal TotalTimeMs { get; }
        public DateTimeOffset StartedTypingAt { get; }
        public DateTime SubmittedAtUtc { get; }
        public IEnumerable<KeyPressEvent> Events { get; }
    }
}
