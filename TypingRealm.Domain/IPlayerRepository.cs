namespace TypingRealm.Domain;

public interface IPlayerRepository
{
    Player? Find(PlayerId playerId);
    void Save(Player player);
}
