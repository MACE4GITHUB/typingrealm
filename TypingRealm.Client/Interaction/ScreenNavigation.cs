using System;

namespace TypingRealm.Client.Interaction
{
    public sealed class ScreenNavigation : IScreenNavigation
    {
        public GameScreen Screen { get; set; } = GameScreen.MainMenu;
        public ModalModule ActiveModalModule { get; set; }
        public ModalModule BackgroundModalModule { get; set; }

        private readonly DialogModalScreenHandler _dialogModalScreenHandler;

        public ScreenNavigation(DialogModalScreenHandler dialogModalScreenHandler)
        {
            _dialogModalScreenHandler = dialogModalScreenHandler;
        }

        public void OpenModalDialog(string text, Action ok)
        {
            if (ActiveModalModule != ModalModule.None)
                throw new InvalidOperationException();

            _dialogModalScreenHandler.Initialize(text, ok, () => ActiveModalModule = ModalModule.None);

            ActiveModalModule = ModalModule.Dialog;
        }

        public void CloseActiveModalModule()
        {
            ActiveModalModule = BackgroundModalModule;
            BackgroundModalModule = ModalModule.None;
        }

        public void SwitchBetweenModalModules()
        {
            var bg = BackgroundModalModule;
            BackgroundModalModule = ActiveModalModule;
            ActiveModalModule = bg;
        }
    }
}
