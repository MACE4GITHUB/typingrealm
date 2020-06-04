using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class Attack
    {
#pragma warning disable CS8618
        public Attack() { }
#pragma warning restore CS8618
        public Attack(string playerId) => PlayerId = playerId;

        public string PlayerId { get; set; }
    }
}
