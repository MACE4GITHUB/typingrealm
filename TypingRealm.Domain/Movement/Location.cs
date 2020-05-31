using System.Collections.Generic;

namespace TypingRealm.Domain.Movement
{
    public sealed class Location
    {
        public Location(IEnumerable<LocationId> locations)
        {
            Locations = locations;
        }

        public IEnumerable<LocationId> Locations { get; }
    }
}
