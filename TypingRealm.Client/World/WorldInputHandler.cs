using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;

        public WorldInputHandler(
            ITyperPool typerPool,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager) : base(typerPool)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
        }

        protected override void OnTyped(Typer typer)
        {
            base.OnTyped(typer);

            if (typer == TyperPool.GetByKey("disconnect"))
            {
                _connectionManager.DisconnectFromWorld();
                _screenNavigation.Screen = GameScreen.MainMenu;
                return;
            }

            // Handle other actions.
        }
    }
}
