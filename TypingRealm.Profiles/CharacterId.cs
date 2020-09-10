using System;
using TypingRealm.Common;

namespace TypingRealm.Profiles
{
    public sealed class CharacterId : Identity
    {
        public CharacterId(string value) : base(value)
        {
        }

        public static CharacterId New()
        {
            return new CharacterId($"{nameof(CharacterId)}-{Guid.NewGuid()}");
        }
    }
}
