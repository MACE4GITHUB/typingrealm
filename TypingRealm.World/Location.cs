using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging;
using TypingRealm.World.Activities.RopeWar;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    [Message]
    public sealed class WorldState
    {
        public string LocationId { get; set; }
        public bool CanProposeRopeWar { get; set; }
        public List<RopeWarActivityState> RopeWarActivities { get; set; }
        public List<string> Characters { get; set; }
        public List<string> Locations { get; set; }
    }

    public sealed class Location
    {
        public string LocationId { get; set; }
        public bool CanProposeRopeWar { get; set; }
        public List<RopeWarActivity> RopeWarActivities { get; set; }
        public List<string> Characters { get; set; }
        public List<string> Locations { get; set; }

        public RopeWarActivity? GetRopeWarFor(string characterId)
        {
            return RopeWarActivities.FirstOrDefault(rw => rw.HasParticipant(characterId));
        }

        public void VoteToStartRopeWar(string characterId)
        {
            var ropeWar = GetRopeWarFor(characterId);
            if (ropeWar == null)
                throw new InvalidOperationException("Rope war does not exist for this character.");

            if (ropeWar.HasStarted)
                throw new InvalidOperationException("Rope war has already started.");

            ropeWar.VoteToStart(characterId);
        }

        public WorldState GetWorldState()
        {
            return new WorldState
            {
                LocationId = LocationId,
                CanProposeRopeWar = CanProposeRopeWar,
                RopeWarActivities = RopeWarActivities.Select(activity => activity.GetState()).ToList(),
                Characters = Characters,
                Locations = Locations
            };
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
