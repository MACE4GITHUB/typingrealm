using System.Linq;
using AutoFixture.Kernel;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Tests.Customizations
{
    public sealed class LocationWithAnotherLocation : ISpecimenBuilder
    {
        private readonly LocationId _locationId;

        public LocationWithAnotherLocation(LocationId locationId)
        {
            _locationId = locationId;
        }

        public object Create(object request, ISpecimenContext context)
        {
            return new Location(new[] { _locationId }, Enumerable.Empty<RoadId>());
        }
    }
}
