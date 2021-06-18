using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldState
    {
        public WorldState(Typer disconnectTyper, string locationId)
        {
            DisconnectTyper = disconnectTyper;
            LocationId = locationId;
        }

        public Typer DisconnectTyper { get; }
        public string LocationId { get; }
    }
}
