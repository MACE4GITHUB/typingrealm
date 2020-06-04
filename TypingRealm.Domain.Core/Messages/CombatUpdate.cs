using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class CombatUpdate
    {
#pragma warning disable CS8618
        public CombatUpdate() { }
#pragma warning restore CS8618
        public CombatUpdate(string enemyId)
        {
            EnemyId = enemyId;
        }

        public string EnemyId { get; set; }
    }
}
