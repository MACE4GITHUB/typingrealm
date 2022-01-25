using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages;

[Message]
public sealed class TeleportPlayerToLocation
{
#pragma warning disable CS8618
    public TeleportPlayerToLocation() { }
#pragma warning restore CS8618
    public TeleportPlayerToLocation(string playerId, string locationId)
    {
        PlayerId = playerId;
        LocationId = locationId;
    }

    public string PlayerId { get; set; }
    public string LocationId { get; set; }
}
