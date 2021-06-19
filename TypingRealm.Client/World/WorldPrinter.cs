using TypingRealm.Client.Data;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.World
{
    public sealed class WorldPrinter : IPrinter<WorldScreenState>
    {
        private readonly IOutput _output;
        private readonly ICharacterService _characterService;
        private readonly ILocationService _locationService;

        public WorldPrinter(
            IOutput output,
            ICharacterService characterService,
            ILocationService locationService)
        {
            _output = output;
            _characterService = characterService;
            _locationService = locationService;
        }

        public void Print(WorldScreenState state)
        {
            if (state == null)
            {
                _output.WriteLine("LOADING...");
                return;
            }

            var location = _locationService.GetLocation(state.LocationId);
            _output.WriteLine("World:");
            _output.WriteLine();
            _output.Write("Your current location: ");
            _output.Write(location.Name);
            _output.WriteLine();

            _output.Write("Disconnect -    ");
            _output.Write(state.DisconnectTyper);
        }
    }
}
