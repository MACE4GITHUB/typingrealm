namespace TypingRealm.Client.Interaction
{
    public interface IScreenNavigation
    {
        GameScreen Screen { get; set; }
        ModalModule ActiveModalModule { get; set; }
        ModalModule BackgroundModalModule { get; set; }

        void SwitchBetweenModalModules();
        void CloseActiveModalModule();
    }
}
