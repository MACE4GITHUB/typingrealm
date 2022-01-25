namespace TypingRealm.Domain.Movement;

/// <summary>
/// Every road has two points. Distance between points is subjective to the
/// direction of movement.
/// </summary>
public sealed class RoadPoint
{
    public RoadPoint(LocationId locationId, Distance distanceToOppositeLocation)
    {
        LocationId = locationId;
        DistanceToOppositeLocation = distanceToOppositeLocation;
    }

    /// <summary>
    /// Location of this point.
    /// </summary>
    public LocationId LocationId { get; }

    /// <summary>
    /// Distance from the location of this point to another point.
    /// </summary>
    public Distance DistanceToOppositeLocation { get; }
}
