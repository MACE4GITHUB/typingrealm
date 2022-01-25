using TypingRealm.Messaging;

#pragma warning disable CS8618
namespace TypingRealm.World.Activities.RopeWar;

[Message]
public sealed class JoinRopeWarContest
{
    public string RopeWarId { get; set; }
    public RopeWarSide Side { get; set; }
}
#pragma warning restore CS8618
