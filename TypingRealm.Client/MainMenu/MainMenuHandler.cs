using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.MainMenu
{
    public interface IMainMenuHandler
    {
        void SwitchToCharacterCreationScreen();
        void ConnectAsCharacter(string characterId);
    }

    public sealed class MainMenuHandler : IMainMenuHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;

        public MainMenuHandler(
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
        }

        public void ConnectAsCharacter(string characterId)
        {
            _connectionManager.ConnectToWorld(characterId);

            // Maybe set screen to some intermediatery "loading" value. and after connection screen should be determined by received state.
            // TODO: See if after connecting to world the character is actually in some activity - and select this activity screen instead of world screen.
            _screenNavigation.Screen = GameScreen.World;
        }

        public void SwitchToCharacterCreationScreen()
        {
            _screenNavigation.Screen = GameScreen.CharacterCreation;
        }
    }
}
