using TypingRealm.Common;

namespace TypingRealm.Domain;

public sealed class PlayerId : Identity
{
    public PlayerId(string value) : base(value)
    {
    }
}
