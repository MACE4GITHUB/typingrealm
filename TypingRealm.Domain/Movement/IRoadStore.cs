namespace TypingRealm.Domain.Movement
{
    public interface IRoadStore
    {
        Road? Find(RoadId roadId);
    }
}
