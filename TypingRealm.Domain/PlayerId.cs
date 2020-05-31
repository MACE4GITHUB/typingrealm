using System;
using TypingRealm.Domain.Common;

namespace TypingRealm.Domain
{
    public sealed class PlayerId : Identity<string>
    {
        public PlayerId(string value) : base(value)
        {
        }

        public static PlayerId New()
        {
            return new PlayerId(Guid.NewGuid().ToString());
        }
    }
}
