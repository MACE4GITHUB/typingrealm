using System;

namespace TypingRealm.Client.Interaction
{
    public sealed class ScreenNavigation : IScreenNavigation
    {
        public GameScreen Screen { get; set; } = GameScreen.MainMenu;
        public ModalModule ActiveModalModule { get; set; }
        public ModalModule BackgroundModalModule { get; set; }
        public IScreenHandler? Dialog { get; set; }

        private readonly DialogScreenHandler _dialogModalScreenHandler;

        public ScreenNavigation(DialogScreenHandler dialogModalScreenHandler)
        {
            _dialogModalScreenHandler = dialogModalScreenHandler;
        }

        public void OpenDialog(string text, Action ok, Action cancel)
        {
            if (Dialog != null)
                return; // Do not override existing dialog.

            _dialogModalScreenHandler.Initialize(text, OkAction, CancelAction);
            Dialog = _dialogModalScreenHandler;

            void OkAction()
            {
                ok();
                Dialog = null;
            }

            void CancelAction()
            {
                cancel();
                Dialog = null;
            }
        }
    }
}
