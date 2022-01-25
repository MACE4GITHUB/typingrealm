using System;
using System.Collections.Generic;

namespace TypingRealm.Profiles.Activities;

public sealed record Activity(
    string ActivityId,
    ActivityType Type,
    IEnumerable<string> CharacterIds)
{
    public bool IsFinished { get; private set; }

    public void Finish()
    {
        if (IsFinished)
            throw new InvalidOperationException("Activity has already been finished.");

        IsFinished = true;
    }
}
