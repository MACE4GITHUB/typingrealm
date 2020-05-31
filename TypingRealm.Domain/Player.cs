namespace TypingRealm.Domain
{
    public sealed class Player
    {
        public Player(PlayerId playerId, string name)
        {
            PlayerId = playerId;
            Name = name;
            LocationId = new LocationId("Starting location");
        }

        public PlayerId PlayerId { get; }
        public string Name { get; }
        public LocationId LocationId { get; private set; }

        public void MoveToLocation(LocationId locationId)
        {
            LocationId = locationId;
        }

        public string GetUniquePlayerPosition()
        {
            return $"l_{LocationId}";
        }
    }
}
