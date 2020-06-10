using System.Collections.Generic;

namespace TypingRealm.Domain.Movement
{
    public sealed class Location
    {
        public Location(
            IEnumerable<LocationId> locations,
            IEnumerable<RoadId> roads)
        {
            Locations = locations;
            Roads = roads;
        }

        public IEnumerable<LocationId> Locations { get; }
        public IEnumerable<RoadId> Roads { get; }
    }
}
