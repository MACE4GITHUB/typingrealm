namespace TypingRealm.Client.Data
{
    public interface ICharacterService
    {
        public string GetCharacterName(string characterId);
    }

    public sealed record Location(
        string LocationId,
        string Name,
        string Description);

    public interface ILocationService
    {
        public Location GetLocation(string locationId);
    }

    public sealed class StubLocationService : ILocationService
    {
        public Location GetLocation(string locationId)
        {
            return new Location(locationId, $"{locationId}-name", "some description of the location");
        }
    }
}
