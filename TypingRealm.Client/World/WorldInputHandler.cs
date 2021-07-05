using System.Linq;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.World.Movement;

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

            var entrance = _state.LocationEntrances.FirstOrDefault(
                e => e.Typer == typer);

            if (entrance != null)
            {
                _ = _connectionManager.WorldConnection?.SendAsync(new MoveToLocation
                {
                    LocationId = entrance.LocationId
                }, default);
                return;
            }

            // Handle other actions.
        }
    }
}
