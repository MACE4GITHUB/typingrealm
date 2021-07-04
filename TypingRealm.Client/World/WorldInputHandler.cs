using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;
        private readonly WorldScreenState _state;

        public WorldInputHandler(
            ITyperPool typerPool,
            ComponentPool componentPool,
            WorldScreenState state,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager) : base(typerPool, componentPool)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
            _state = state;
        }

        protected override void OnTyped(Typer typer)
        {
            base.OnTyped(typer);

            if (typer == _state.Disconnect)
            {
                _connectionManager.DisconnectFromWorld();
                _screenNavigation.Screen = GameScreen.MainMenu;
                return;
            }

            // Handle other actions.
        }
    }
}
