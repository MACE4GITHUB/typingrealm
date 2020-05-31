using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class MoveToLocation
    {
#pragma warning disable CS8618
        public MoveToLocation() { }
#pragma warning restore CS8618
        public MoveToLocation(string locationId) => LocationId = locationId;

        public string LocationId { get; set; }
    }
}
