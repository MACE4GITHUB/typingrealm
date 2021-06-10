using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.World.Activities;
using TypingRealm.World.Layers;

namespace TypingRealm.World
{
    public sealed class LocationStore : ILocationStore, IActivityStore
    {
        private readonly List<Location> _locations = new List<Location>()
        {
            Location.FromPersistenceState(new LocationPersistenceState
            {
                LocationId = "1",
                Locations = new HashSet<string> { "2" },
                AllowedActivityTypes = new HashSet<Activities.ActivityType>(),
                Characters = new HashSet<string>(),
                Activities = new HashSet<Activity>()
            }),
            Location.FromPersistenceState(new LocationPersistenceState
            {
                LocationId = "2",
                Locations = new HashSet<string> { "1" },
                AllowedActivityTypes = new HashSet<Activities.ActivityType>
                {
                    ActivityType.RopeWar
                },
                Characters = new HashSet<string>(),
                Activities = new HashSet<Activity>()
            })
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

            // TODO: Get descriptors instead of actual objects that can be modified.
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
