using System.Collections.Generic;

namespace TypingRealm.Domain.Movement
{
    public sealed class Location
    {
        public Location(
            LocationId locationId,
            IEnumerable<LocationId> locations,
            IEnumerable<RoadId> roads)
        {
            LocationId = locationId;
            Locations = locations;
            Roads = roads;
        }

        public LocationId LocationId { get; }
        public IEnumerable<LocationId> Locations { get; }
        public IEnumerable<RoadId> Roads { get; }
    }
}
