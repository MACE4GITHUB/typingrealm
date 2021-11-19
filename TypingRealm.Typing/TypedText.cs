using System;
using System.Collections.Generic;

namespace TypingRealm.Typing
{
    /// <summary>
    /// An entity that can be submitted for automatic creation of text & sessions.
    /// </summary>
    public sealed record TypedText(
        string Value,
        decimal TotalTimeMs,
        DateTime StartedTypingUtc,
        int UserTimeZoneOffsetMinutes,
        IEnumerable<KeyPressEvent> Events);
}
