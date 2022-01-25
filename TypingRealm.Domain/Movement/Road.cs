namespace TypingRealm.Domain.Movement;

public sealed class Road
{
    public Road(RoadId roadId, RoadPoint fromPoint, RoadPoint toPoint)
    {
        RoadId = roadId;
        FromPoint = fromPoint;
        ToPoint = toPoint;
    }

    public RoadId RoadId { get; }
    public RoadPoint FromPoint { get; }
    public RoadPoint ToPoint { get; }
}
