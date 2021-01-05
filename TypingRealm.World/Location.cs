using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    [Message]
    public sealed class Location
    {
        public string LocationId { get; set; }
        public bool CanProposeRopeWar { get; set; }
        public List<RopeWar> RopeWars { get; set; }
        public List<string> Characters { get; set; }
        public List<string> Locations { get; set; }

        public RopeWar? GetRopeWarFor(string characterId)
        {
            return RopeWars.FirstOrDefault(rw => rw.HasParticipant(characterId));
        }

        public void VoteToStartRopeWar(string characterId)
        {
            var ropeWar = GetRopeWarFor(characterId);
            if (ropeWar == null)
                throw new InvalidOperationException("Rope war does not exist for this character.");

            if (ropeWar.HasStarted)
                throw new InvalidOperationException("Rope war has already started.");

            if (ropeWar.Votes.Contains(characterId))
                throw new InvalidOperationException("Already voted.");

            ropeWar.Votes.Add(characterId);

            // TODO: If all votes from all players are gathered - start the rope war: set HasStarted to true.
            // Client will see this in another update and connect to RopeWar server accordingly.
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
