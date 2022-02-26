using TypingRealm.Messaging;

namespace TypingRealm.TypingDuels;

[Message]
public sealed class Typed
{
    public int TypedCharactersCount { get; set; }
    public string? ClientId { get; set; }
}
