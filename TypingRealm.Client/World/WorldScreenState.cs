namespace TypingRealm.Client.World
{
    public sealed record LocationInfo(
        string LocationId,
        string Name,
        string Description);

    public sealed class WorldScreenState
    {
        public WorldScreenState(LocationInfo currentLocation)
        {
            CurrentLocation = currentLocation;
        }

        public LocationInfo CurrentLocation { get; set; }
    }
}
