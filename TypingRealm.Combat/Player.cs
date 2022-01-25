namespace TypingRealm.Combat;

public sealed class Player
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public Player() { } // For serialization.
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
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
