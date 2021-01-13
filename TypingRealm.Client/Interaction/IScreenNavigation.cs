using System;

namespace TypingRealm.Client.Interaction
{
    public interface IScreenNavigation
    {
        GameScreen Screen { get; set; }
        ModalModule ActiveModalModule { get; set; }
        ModalModule BackgroundModalModule { get; set; }

        void OpenModalDialog(string text, Action ok);
        void SwitchBetweenModalModules();
        void CloseActiveModalModule();
    }
}
