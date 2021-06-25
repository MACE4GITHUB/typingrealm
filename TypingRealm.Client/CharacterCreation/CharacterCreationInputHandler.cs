using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;

        public CharacterCreationInputHandler(
            ITyperPool typerPool,
            IScreenNavigation screenNavigation) : base(typerPool)
        {
            _screenNavigation = screenNavigation;
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == TyperPool.GetByKey("back"))
            {
                SwitchBackToMainMenu();
                return;
            }
        }

        private void SwitchBackToMainMenu()
        {
            _screenNavigation.Screen = GameScreen.MainMenu;
        }
    }
}
