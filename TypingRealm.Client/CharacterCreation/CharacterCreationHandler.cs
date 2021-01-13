using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.CharacterCreation
{
    public interface ICharacterCreationHandler
    {
        void CloseAndReturnToMainMenu();
    }

    public sealed class CharacterCreationHandler : ICharacterCreationHandler
    {
        private readonly IScreenNavigation _screenNavigation;

        public CharacterCreationHandler(IScreenNavigation screenNavigation)
        {
            _screenNavigation = screenNavigation;
        }

        public void CloseAndReturnToMainMenu()
        {
            _screenNavigation.OpenModalDialog(
                "Are you sure you want to exit to main menu?",
                () => _screenNavigation.Screen = GameScreen.MainMenu);
        }
    }
}
