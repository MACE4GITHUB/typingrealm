using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages;

[Message]
public sealed class Move
{
#pragma warning disable CS8618
    public Move() { }
#pragma warning restore CS8618
    public Move(int distance) => Distance = distance;

    public int Distance { get; set; }
}
