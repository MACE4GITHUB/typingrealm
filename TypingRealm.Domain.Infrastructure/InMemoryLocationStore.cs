using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryLocationStore : ILocationStore
    {
        private readonly Dictionary<LocationId, Location> _locations
            = new Dictionary<LocationId, Location>
            {
                [new LocationId("village")] = new Location(
                new LocationId("village"),
                new[]
                {
                    new LocationId("old house"),
                    new LocationId("marketplace")
                }, new[]
                {
                    new RoadId("village-to-forest")
                }),
                [new LocationId("old house")] = new Location(
                new LocationId("old house"),
                new[]
                {
                    new LocationId("village")
                }, Enumerable.Empty<RoadId>()),
                [new LocationId("marketplace")] = new Location(
                new LocationId("marketplace"),
                new[]
                {
                    new LocationId("village")
                }, Enumerable.Empty<RoadId>()),
                [new LocationId("forest")] = new Location(
                    new LocationId("forest"),
                    Array.Empty<LocationId>(),
                    Enumerable.Empty<RoadId>())
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
