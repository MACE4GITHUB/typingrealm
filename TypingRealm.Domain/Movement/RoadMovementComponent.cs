using System;

namespace TypingRealm.Domain.Movement
{
    public sealed class MovementComponent
    {
        private MovementComponent(Road road, Distance progress, RoadDirection direction)
        {
            Road = road;
            Progress = progress;
            Direction = direction;
        }

        public Road Road { get; }
        public Distance Progress { get; }
        public RoadDirection Direction { get; }

        public LocationId ArrivalLocationId => Road.GetArrivalLocationId(Direction);
        public Distance Distance => Road.GetDistanceFor(Direction);
        public bool HasArrived => Progress == Distance;

        public MovementComponent Move(Distance progress)
        {
            var newProgress = Progress + progress;
            var distance = Road.GetDistanceFor(Direction);

            if (newProgress > distance)
                throw new InvalidOperationException("Can't progress beyond distance.");

            return new MovementComponent(
                Road, newProgress, Direction);
        }

        public MovementComponent TurnAround()
        {
            var newDirection = Direction.Flip();

            if (Progress.IsZero)
            {
                var newDistance = Road.GetDistanceFor(newDirection);
                return new MovementComponent(Road, newDistance, newDirection);
            }

            // Math.
            {
                // Don't remove 100d part. It specifies decimal type here.
                var oldDistanceValue = Road.GetDistanceFor(Direction).Value;
                var oldProgressValue = Progress.Value;
                var oldProgressPercentage = oldProgressValue * 100d / oldDistanceValue;
                var newProgressPercentage = 100 - oldProgressPercentage;

                var newDistanceValue = Road.GetDistanceFor(newDirection).Value;
                var newProgressValue = newDistanceValue * newProgressPercentage / 100d;
                var newProgress = new Distance(Round(newProgressValue));

                return new MovementComponent(Road, newProgress, newDirection);
            }
        }

        public static MovementComponent EnterRoadFrom(Road road, LocationId locationId)
        {
            if (road.FromPoint.LocationId == locationId)
                return new MovementComponent(road, Distance.Zero, RoadDirection.Forward);

            if (road.ToPoint.LocationId == locationId)
                return new MovementComponent(road, Distance.Zero, RoadDirection.Backward);

            throw new InvalidOperationException($"Can't enter road {road.RoadId} from location {locationId}.");
        }

        // TODO: Move to common place (Common assembly for instance).
        private static int Round(double value)
        {
            return (int)Math.Floor(value);
        }
    }
}
