namespace TypingRealm.Client.Interaction
{
    public sealed class ScreenNavigation : IScreenNavigation
    {
        public GameScreen Screen { get; set; } = GameScreen.MainMenu;
        public ModalModule ActiveModalModule { get; set; }
        public ModalModule BackgroundModalModule { get; set; }

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
