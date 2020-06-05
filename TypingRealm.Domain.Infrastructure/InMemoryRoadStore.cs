using System.Collections.Generic;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryRoadStore : IRoadStore
    {
        private readonly Dictionary<RoadId, Road> _roads
            = new Dictionary<RoadId, Road>
            {
                [new RoadId("village-to-forest")] = new Road(
                    new LocationId("village"),
                    new LocationId("forest"),
                    100),
                [new RoadId("forest-to-village")] = new Road(
                    new LocationId("forest"),
                    new LocationId("village"),
                    50)
            };

        public Road? Find(RoadId roadId)
        {
            if (!_roads.ContainsKey(roadId))
                return null;

            return _roads[roadId];
        }
    }
}
