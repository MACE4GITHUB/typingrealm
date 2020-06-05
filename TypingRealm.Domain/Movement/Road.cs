namespace TypingRealm.Domain.Movement
{
    public sealed class Road
    {
        public Road(
            LocationId fromLocationId,
            LocationId toLocationId,
            int distance)
        {
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
            Distance = distance;
        }

        public LocationId FromLocationId { get; }
        public LocationId ToLocationId { get; }
        public int Distance { get; }
    }
}
