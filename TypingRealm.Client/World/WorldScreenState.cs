using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenState
    {
        public WorldScreenState(Typer disconnectTyper, string locationId)
        {
            DisconnectTyper = disconnectTyper;
            LocationId = locationId;
        }

        public Typer DisconnectTyper { get; }
        public string LocationId { get; }
    }
}
