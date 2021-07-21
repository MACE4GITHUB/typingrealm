using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Data.Infrastructure
{
    public sealed class Location
    {
    }

    public interface ILocationRepository
    {
        ValueTask<Location?> FindAsync(string locationId, CancellationToken cancellationToken);
    }

    public sealed class InMemoryLocationRepository : ILocationRepository
    {
        internal readonly Dictionary<string, Location> _cache
            = new Dictionary<string, Location>();

        public InMemoryLocationRepository()
        {
            _cache.Add("1", new Location());
        }

        public ValueTask<Location?> FindAsync(string locationId, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(locationId, out var location);
            return new ValueTask<Location?>(location);
        }
    }
}
