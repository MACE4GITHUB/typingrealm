using TypingRealm.Messaging.Messages;

namespace TypingRealm.Combat.Messages;
#pragma warning disable CS8618
public sealed class TargetingPlayer : BroadcastMessage
{
    public string TargetId { get; set; }
}

public sealed class TargetedPlayer : BroadcastMessage
{
    public string TargetId { get; set; }
}

public sealed class Attacking : BroadcastMessage
{
    public string TargetId { get; set; }
    public BodyPart BodyPart { get; set; }
}

public sealed class Attacked : BroadcastMessage
{
    public string TargetId { get; set; }
    public BodyPart BodyPart { get; set; }
}
