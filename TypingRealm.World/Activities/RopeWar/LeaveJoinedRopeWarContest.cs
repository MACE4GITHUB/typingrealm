using TypingRealm.Messaging;

namespace TypingRealm.World.Activities.RopeWar
{
    [Message]
    public sealed class LeaveJoinedRopeWarContest
    {
        // Should return money to player if it's not started yet, otherwise player loses money.
        // Should reset "vote" to start the contest.
        // Should delete the contest if it was the last player.
    }
}
