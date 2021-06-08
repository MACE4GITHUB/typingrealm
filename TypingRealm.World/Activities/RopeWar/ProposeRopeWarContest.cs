using TypingRealm.Messaging;

#pragma warning disable CS8618
namespace TypingRealm.World.Activities.RopeWar
{
    [Message]
    public sealed class ProposeRopeWarContest
    {
        public string Name { get; set; }
        public long Bet { get; set; }
        public RopeWarSide Side { get; set; }
    }
}
#pragma warning restore CS8618
