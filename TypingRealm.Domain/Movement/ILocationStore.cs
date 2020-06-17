namespace TypingRealm.Domain.Movement
{
    public interface ILocationStore
    {
        Location? Find(LocationId locationId);
    }
}
