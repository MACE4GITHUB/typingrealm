using TypingRealm.Messaging;

namespace TypingRealm.Combat.Messages
{
#pragma warning disable CS8618
    [Message]
    public sealed class Engage
    {
        public string PlayerId { get; set; }
        public string CombatRoomId { get; set; }
    }
#pragma warning restore CS8618
}
