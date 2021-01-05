#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.World
{
    public sealed class LocationStore : ILocationStore
    {
        private readonly List<Location> _locations = new List<Location>()
        {
            new Location
            {
                LocationId = "1",
                Locations = new List<string> { "2" },
                CanProposeRopeWar = false,
                Characters = new List<string>(),
                RopeWars = new List<RopeWar>()
            },
            new Location
            {
                LocationId = "2",
                Locations = new List<string> { "1" },
                CanProposeRopeWar = true,
                Characters = new List<string>(),
                RopeWars = new List<RopeWar>()
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

        public Location FindStartingLocation()
        {
            return _locations.FirstOrDefault(l => l.LocationId == "1")!;
        }

        public void Save(Location location)
        {
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
