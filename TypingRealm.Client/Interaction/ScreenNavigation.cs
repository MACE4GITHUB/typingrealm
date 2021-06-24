using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TypingRealm.Client.Interaction
{
    public sealed class ScreenNavigation : IScreenNavigation
    {
        private readonly BehaviorSubject<GameScreen> _screenSubject;
        private readonly DialogScreenHandler _dialogScreenHandler;

        public ModalModule ActiveModalModule { get; set; }
        public ModalModule BackgroundModalModule { get; set; }
        public IScreenHandler? Dialog { get; set; }

        public GameScreen Screen
        {
            get => _screenSubject.Value;
            set => _screenSubject.OnNext(value);
        }

        public ScreenNavigation(DialogScreenHandler dialogScreenHandler)
        {
            _dialogScreenHandler = dialogScreenHandler;
            _screenSubject = new BehaviorSubject<GameScreen>(GameScreen.MainMenu);
        }

        public IObservable<GameScreen> ScreenObservable => _screenSubject
            .Where(screen => screen != GameScreen.Exit);

        public void OpenDialog(string text, Action ok, Action cancel)
        {
            if (Dialog != null)
                return; // Do not override existing dialog.

            _dialogScreenHandler.Initialize(text, OkAction, CancelAction);
            Dialog = _dialogScreenHandler;

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
