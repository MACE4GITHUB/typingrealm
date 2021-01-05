using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    [Message]
    public sealed class ProposeRopeWarContest
    {
        public string Name { get; set; }
        public int Bet { get; set; }
        public RopeWarSide Side { get; set; }

        // TODO: Generalize this: CANNOT join ACTIVITY when already in another activity.
        // Should check if currently engaged in any other activity - then disallow joining another activity.
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
