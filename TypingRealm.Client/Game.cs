using TypingRealm.Client.MainMenu;

namespace TypingRealm.Client
{
    public sealed class Game
    {
        private readonly MainMenuPrinter _mainMenuPrinter;

        public Game(MainMenuPrinter mainMenuPrinter)
        {
            _mainMenuPrinter = mainMenuPrinter;
        }
    }
}
