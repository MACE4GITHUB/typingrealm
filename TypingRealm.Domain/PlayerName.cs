using System;
using TypingRealm.Domain.Common;

namespace TypingRealm.Domain
{
    public sealed class PlayerName : Primitive<string>
    {
        public PlayerName(string value) : base(value)
        {
            if (value.Length < 3)
                throw new ArgumentException("Player name should be at least 3 characters long.", nameof(value));

            if (value.Length > 20)
                throw new ArgumentException("Maximum length of player name is 20 characters.", nameof(value));
        }
    }
}
