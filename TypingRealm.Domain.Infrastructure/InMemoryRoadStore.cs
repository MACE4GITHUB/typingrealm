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
                    new RoadId("village-to-forest"),
                    new RoadPoint(
                        new LocationId("village"),
                        new Distance(100)),
                    new RoadPoint(
                        new LocationId("forest"),
                        new Distance(50)))
            };

        public Road? Find(RoadId roadId)
        {
            if (!_roads.ContainsKey(roadId))
                return null;

            return _roads[roadId];
        }
    }
}
