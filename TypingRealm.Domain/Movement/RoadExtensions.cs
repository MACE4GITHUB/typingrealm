using System;

namespace TypingRealm.Domain.Movement
{
    public static class RoadExtensions
    {
        public static RoadPoint GetStartingPointFor(this Road road, RoadDirection direction)
        {
            if (direction == RoadDirection.Forward)
                return road.FromPoint;

            if (direction == RoadDirection.Backward)
                return road.ToPoint;

            throw new ArgumentException("Unknown direction", nameof(direction));
        }

        public static RoadPoint GetArrivalPointFor(this Road road, RoadDirection direction)
            => road.GetStartingPointFor(direction.Flip());

        public static RoadDirection Flip(this RoadDirection direction)
        {
            if (direction == RoadDirection.Forward)
                return RoadDirection.Backward;

            if (direction == RoadDirection.Backward)
                return RoadDirection.Forward;

            throw new ArgumentException("Invalid direction.", nameof(direction));
        }

        public static Distance GetDistanceFor(this Road road, RoadDirection direction)
            => road.GetStartingPointFor(direction).DistanceToOppositeLocation;

        public static LocationId GetArrivalLocationId(this Road road, RoadDirection direction)
            => road.GetArrivalPointFor(direction).LocationId;
    }
}
