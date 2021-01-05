#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace TypingRealm.World
{
    public interface ILocationStore
    {
        Location? Find(string locationId);
        Location? FindLocationForCharacter(string characterId);
        Location FindStartingLocation();
        void Save(Location location);
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
