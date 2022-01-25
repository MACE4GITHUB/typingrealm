using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages;

[Message]
public sealed class EnterRoad
{
#pragma warning disable CS8618
    public EnterRoad() { }
#pragma warning restore CS8618
    public EnterRoad(string roadId) => RoadId = roadId;

    public string RoadId { get; set; }
}
