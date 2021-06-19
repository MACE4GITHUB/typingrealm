using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenHandler : ScreenHandler<WorldScreenState>
    {
        private readonly Typer _disconnectTyper;
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;

        public WorldScreenHandler(
            ITextGenerator textGenerator,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager,
            IPrinter<WorldScreenState> printer) : base(textGenerator, printer)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
            _disconnectTyper = MakeUniqueTyper();
        }

        protected override WorldScreenState GetCurrentScreenState()
        {
            var currentWorldState = _connectionManager.CurrentWorldState;
            /*if (currentWorldState == null)
                throw new NotImplementedException("State is null."); // TODO: Print "LOADING" screen.*/
            if (currentWorldState == null)
                return null!;

            return new WorldScreenState(
                _disconnectTyper, currentWorldState.LocationId);
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == _disconnectTyper)
            {
                _connectionManager.DisconnectFromWorld();
                _screenNavigation.Screen = GameScreen.MainMenu;
                return;
            }

            // Handle other actions.
        }
    }
}
