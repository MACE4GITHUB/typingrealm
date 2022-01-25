using System;

namespace TypingRealm.Profiles;

public sealed class CharacterName : Primitive<string>
{
    public CharacterName(string value) : base(value)
    {
        if (value.Length < 3)
            throw new ArgumentException("Character name should be at least 3 characters long.", nameof(value));
    }
}
