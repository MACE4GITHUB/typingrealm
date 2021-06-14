using System;

namespace TypingRealm.Client.Interaction
{
    public interface IScreenNavigation
    {
        GameScreen Screen { get; set; }
        ModalModule ActiveModalModule { get; set; }
        ModalModule BackgroundModalModule { get; set; }

        /// <summary>
        /// If this is set - the dialog is forced on top of everything else.
        /// </summary>
        IScreenHandler? Dialog { get; set; }

        void OpenDialog(string text, Action ok, Action cancel);
    }
}
