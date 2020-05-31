using System.Collections.Generic;

namespace TypingRealm.Domain.Movement
{
    public sealed class InMemoryLocationStore : ILocationStore
    {
        private readonly Dictionary<LocationId, Location> _locations
            = new Dictionary<LocationId, Location>
            {
                [new LocationId("village")] = new Location(new[]
                {
                    new LocationId("old house"),
                    new LocationId("marketplace")
                }),
                [new LocationId("old house")] = new Location(new[]
                {
                    new LocationId("village")
                }),
                [new LocationId("marketplace")] = new Location(new[]
                {
                    new LocationId("village")
                })
            };

        public Location? Find(LocationId locationId)
        {
            if (!_locations.ContainsKey(locationId))
                return null;

            return _locations[locationId];
        }

        public LocationId GetStartingLocationId()
        {
            return new LocationId("village");
        }
    }
}
