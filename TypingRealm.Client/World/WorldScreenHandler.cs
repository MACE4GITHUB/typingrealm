using System;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenHandler : ScreenHandler<WorldState>
    {
        private readonly Typer _disconnectTyper;
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;

        public WorldScreenHandler(
            ITextGenerator textGenerator,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager,
            IPrinter<WorldState> printer) : base(textGenerator, printer)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
            _disconnectTyper = MakeUniqueTyper();
        }

        protected override WorldState GetCurrentState()
        {
            var currentWorldState = _connectionManager.CurrentWorldState;
            /*if (currentWorldState == null)
                throw new NotImplementedException("State is null."); // TODO: Print "LOADING" screen.*/
            if (currentWorldState == null)
                return null!;

            return new WorldState(
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
