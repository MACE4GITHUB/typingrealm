#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.World.Activities.RopeWar;
using TypingRealm.World.Layers;

namespace TypingRealm.World
{
    public sealed class LocationStore : ILocationStore, IActivityStore
    {
        private readonly List<Location> _locations = new List<Location>()
        {
            new Location
            {
                LocationId = "1",
                Locations = new List<string> { "2" },
                CanProposeRopeWar = false,
                Characters = new List<string>(),
                RopeWarActivities = new List<RopeWarActivity>()
            },
            new Location
            {
                LocationId = "2",
                Locations = new List<string> { "1" },
                CanProposeRopeWar = true,
                Characters = new List<string>(),
                RopeWarActivities = new List<RopeWarActivity>()
            }
        };

        public Location? Find(string locationId)
        {
            return _locations.FirstOrDefault(l => l.LocationId == locationId);
        }

        public Location? FindLocationForCharacter(string characterId)
        {
            return _locations.FirstOrDefault(l => l.Characters.Contains(characterId));
        }

        public Location FindStartingLocation(string characterId)
        {
            return _locations.FirstOrDefault(l => l.LocationId == "1")!;
        }

        public ValueTask<Stack<Activity>> GetActivitiesForCharacterAsync(string characterId, CancellationToken cancellationToken)
        {
            var location = _locations.FirstOrDefault(l => l.Characters.Contains(characterId));
            if (location == null)
                throw new InvalidOperationException("Location for this character does not exist.");

            // TODO: Query total list of activities, not just ropewar activities.
            // And somehow get the STACK of activities for the character?..
            var activities = location.RopeWarActivities.Where(x => x.LeftSideParticipants.Contains(characterId) || x.RightSideParticipants.Contains(characterId));

            return new ValueTask<Stack<Activity>>(new Stack<Activity>(activities));
        }

        public void Save(Location location)
        {
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
