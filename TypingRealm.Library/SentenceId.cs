using System;
using TypingRealm.Common;

namespace TypingRealm.Library
{
    public sealed class SentenceId : Identity
    {
        public SentenceId(string value) : base(value)
        {
        }

        public static SentenceId New() => new(Guid.NewGuid().ToString());
    }
}
