using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Typing;

/// <summary>
/// An entity that can be submitted for automatic creation of text & sessions.
/// </summary>
public sealed record TypedText(
    string TextId,
    DateTime StartedTypingUtc,
    int UserTimeZoneOffsetMinutes,
    IEnumerable<KeyPressEvent> Events)
{
    public decimal TotalTimeMs => Events.Last().AbsoluteDelay;
}
