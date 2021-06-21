using TypingRealm.Messaging;

#pragma warning disable CS8618
namespace TypingRealm.World.Movement
{
    [Message]
    public sealed class MoveToLocation
    {
        public string LocationId { get; set; }
    }
}
#pragma warning restore CS8618
