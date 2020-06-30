using System;

namespace TypingRealm.Domain.Movement
{
    public static class RoadExtensions
    {
        public static RoadPoint GetStartingPointFor(this Road road, MovementDirection direction)
        {
            if (direction == MovementDirection.Forward)
                return road.FromPoint;

            if (direction == MovementDirection.Backward)
                return road.ToPoint;

            throw new ArgumentException("Unknown direction", nameof(direction));
        }

        public static RoadPoint GetArrivalPointFor(this Road road, MovementDirection direction)
            => road.GetStartingPointFor(direction.Flip());

        public static MovementDirection Flip(this MovementDirection direction)
        {
            if (direction == MovementDirection.Forward)
                return MovementDirection.Backward;

            if (direction == MovementDirection.Backward)
                return MovementDirection.Forward;

            throw new ArgumentException("Invalid direction.", nameof(direction));
        }

        public static Distance GetDistanceFor(this Road road, MovementDirection direction)
            => road.GetStartingPointFor(direction).DistanceToOppositeLocation;

        public static LocationId GetArrivalLocationId(this Road road, MovementDirection direction)
            => road.GetArrivalPointFor(direction).LocationId;
    }
}
