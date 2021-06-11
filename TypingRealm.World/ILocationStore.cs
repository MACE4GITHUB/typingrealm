namespace TypingRealm.World
{
    public interface ILocationStore
    {
        Location? Find(string locationId);
        Location? FindLocationForCharacter(string characterId);
        Location FindStartingLocation(string characterId);
        void Save(Location location);
    }
}
