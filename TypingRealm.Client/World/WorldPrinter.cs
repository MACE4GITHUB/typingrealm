using TypingRealm.Client.Output;

namespace TypingRealm.Client.World
{
    public sealed class WorldPrinter : IPrinter<WorldScreenState>
    {
        private readonly IOutput _output;

        public WorldPrinter(IOutput output)
        {
            _output = output;
        }

        public void Print(WorldScreenState state)
        {
            if (state == null)
            {
                _output.WriteLine("LOADING...");
                return;
            }

            _output.WriteLine("World:");
            _output.WriteLine();

            if (state.IsLoading || state.CurrentLocation == null)
            {
                _output.WriteLine("LOADING...");
                _output.WriteLine();

                _output.Write("Disconnect -    ");
                _output.Write(state.Disconnect);
                return;
            }

            _output.Write("Your current location: ");
            _output.WriteLine(state.CurrentLocation.Name);
            _output.WriteLine(state.CurrentLocation.Description);
            _output.WriteLine();

            _output.WriteLine("Possible locations where you can go:");
            foreach (var locationEntrance in state.LocationEntrances)
            {
                _output.Write(locationEntrance.Name);
                _output.Write("   -   ");
                _output.WriteLine(locationEntrance.Typer);
            }

            if (state.CurrentLocation.CanStartRopeWar && state.CreateRopeWarTyper != null)
            {
                _output.WriteLine("Create new rope war (random) and join it: ");
                _output.WriteLine(state.CreateRopeWarTyper);
                _output.WriteLine();
                _output.WriteLine();
                _output.WriteLine("Existing rope wars:");

                foreach (var ropeWarInfo in state.RopeWars)
                {
                    _output.Write($"{ropeWarInfo.Name} - {ropeWarInfo.Bet}   -   ");

                    if (state.CurrentRopeWar == null)
                        _output.WriteLine(ropeWarInfo.Typer);
                }

                _output.WriteLine();
                if (state.CurrentRopeWar != null)
                {
                    _output.WriteLine("Current rope war:");
                    _output.WriteLine(state.CurrentRopeWar.RopeWarInfo.Name);
                    _output.Write("Leave   -   ");
                    _output.WriteLine(state.CurrentRopeWar.LeaveRopeWarTyper);
                    _output.Write("Switch sides   -   ");
                    _output.WriteLine(state.CurrentRopeWar.SwitchSides);
                }
            }

            _output.WriteLine();
            _output.WriteLine();
            _output.Write("Disconnect -    ");
            _output.Write(state.Disconnect);
        }
    }
}
