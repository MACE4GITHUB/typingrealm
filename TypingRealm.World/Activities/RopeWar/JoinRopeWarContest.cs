using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World.Activities.RopeWar
{
    [Message]
    public sealed class JoinRopeWarContest
    {
        public string RopeWarId { get; set; }
        public RopeWarSide Side { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
