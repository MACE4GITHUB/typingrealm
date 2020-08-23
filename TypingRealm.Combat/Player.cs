namespace TypingRealm.Combat
{
    public sealed class Player
    {
        public Player(string playerId, int maxHealth, int health)
        {
            PlayerId = playerId;
            MaxHealth = maxHealth;
            Health = health;
        }

        public string PlayerId { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
    }
}
