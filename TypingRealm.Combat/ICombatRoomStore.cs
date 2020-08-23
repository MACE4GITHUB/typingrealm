namespace TypingRealm.Combat
{
    public interface ICombatRoomStore
    {
        CombatRoom FindInBattle(string playerId);
        CombatRoom FindOrCreate(string combatRoomId, string playerId);
    }
}
