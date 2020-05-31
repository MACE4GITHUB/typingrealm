namespace TypingRealm.Domain
{
    public sealed class Player
    {
        public Player(string id, string name)
        {
            Id = id;
            Name = name;
            LocationId = "Starting location";
        }

        public string Id { get; }
        public string Name { get; }
        public string LocationId { get; private set; }

        public void MoveTo(string locationId)
        {
            LocationId = locationId;
        }

        public string GetUniqueLocation()
        {
            return $"l_{LocationId}";
        }
    }
}
