using System.Collections.Generic;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    // TODO: Create CHARACTER AR and check it's action. Cannot participate in action when already in another action.
    public sealed class RopeWar : Activity
    {
        public int Bet { get; set; }
        public List<string> LeftSideParticipants { get; set; }
        public List<string> RightSideParticipants { get; set; }
        public List<string> Votes { get; set; } = new List<string>();

        public bool HasParticipant(string characterId)
        {
            return LeftSideParticipants.Contains(characterId)
                || RightSideParticipants.Contains(characterId);
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
