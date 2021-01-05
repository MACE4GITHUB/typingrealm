using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    [Message]
    public sealed class JoinRopeWarContest
    {
        public string RopeWarId { get; set; }
        public RopeWarSide Side { get; set; }

        // Should subtract "bet" from character money. Or rather put that money on hold so nobony can spend more money from the account until the end of the ropewar contest.
        // Should check if currently engaged in any other activity - then disallow joining another activity.
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
