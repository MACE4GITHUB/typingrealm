using System;
using System.Collections.Generic;

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
            int TypingSessionTextIndex,
            decimal TotalTimeMs,
            DateTime StartedTypingUtc,
            DateTime SubmittedResultsUtc,
            IEnumerable<KeyPressEvent> Events);
}
