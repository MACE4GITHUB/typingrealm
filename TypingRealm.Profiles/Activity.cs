using System;
using System.Collections.Generic;

namespace TypingRealm.Profiles
{
    public sealed record Activity(string ActivityId, IEnumerable<string> CharacterIds)
    {
        public bool IsFinished { get; private set; }

        public void Finish()
        {
            if (IsFinished)
                throw new InvalidOperationException("Activity has already been finished.");

            IsFinished = true;
        }
    }
}
