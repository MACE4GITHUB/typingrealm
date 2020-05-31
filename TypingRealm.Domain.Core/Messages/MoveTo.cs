using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class MoveTo
    {
#pragma warning disable CS8618
        public MoveTo() { }
#pragma warning restore CS8618
        public MoveTo(string locationId) => LocationId = locationId;

        public string LocationId { get; set; }
    }
}
