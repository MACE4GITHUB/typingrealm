using TypingRealm.Messaging;

namespace TypingRealm.TypingDuels;

#pragma warning disable CS8618
[Message]
public sealed class ConnectToSession
{
    public string TypingSessionId { get; set; }
}
#pragma warning restore CS8618
