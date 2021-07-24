using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Data.Resources;

namespace TypingRealm.Data.Infrastructure
{
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
            _cache.Add("1", new Location
            {
                LocationId = "1",
                LocationEntrances = new List<string> { "2" },
                CanHaveRopeWar = false,

                Name = "Starting village",
                Description = "This is a starting village. There's nothing to do here. Go to the training grounds to participate in rope wars."
            });

            _cache.Add("2", new Location
            {
                LocationId = "2",
                LocationEntrances = new List<string> { "1" },
                CanHaveRopeWar = true,

                Name = "Training grounds",
                Description = "Here you can train - have a rope war contest or something..."
            });
        }

        public ValueTask<Location?> FindAsync(string locationId, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(locationId, out var location);
            return new ValueTask<Location?>(location);
        }
    }
}
