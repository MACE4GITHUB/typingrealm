using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Entity of UserSession.
    ///
    /// Result of typing session. This is an atomic entity that is logged and is
    /// never changed (appended to UserSession). It is validated before logging
    /// and invalid / cheated entries are discarded.
    /// </summary>
    public sealed record TextTypingResult(
        // TODO: Consider making it an AR and include foreing key to UserSession, so that we can query everything just by text typing result id.
        string TextTypingResultId, // A hack to distinguish text typing result uniquely.
        int TypingSessionTextIndex,
        DateTime StartedTypingUtc,
        DateTime SubmittedResultsUtc,
        IEnumerable<KeyPressEvent> Events)
    {
        public decimal TotalTimeMs => Events.Last().AbsoluteDelay;
    }
}
