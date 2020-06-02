using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class Join
    {
#pragma warning disable CS8618
        public Join() { }
#pragma warning restore CS8618
        public Join(string playerId) => PlayerId = playerId;

        public string PlayerId { get; set; }
    }
}
