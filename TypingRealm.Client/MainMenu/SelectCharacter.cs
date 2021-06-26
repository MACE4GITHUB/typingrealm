using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed record SelectCharacter(
        string CharacterId, string Name, Typer Typer)
    {
    }
}
