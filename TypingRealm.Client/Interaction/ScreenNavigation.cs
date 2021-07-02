using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TypingRealm.Client.CharacterCreation;
using TypingRealm.Client.MainMenu;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Client.Interaction
{
    public interface IScreen
    {
        IInputHandler InputHandler { get; }
        IChangeDetector ChangeDetector { get; }
    }

    public interface IScreenFactory
    {
        IScreen OpenScreen(GameScreen screenType);
        void CloseScreen(IScreen screen);
    }

    public sealed class Screen : IScreen
    {
        public Screen(
            string resourceId,
            IInputHandler inputHandler,
            IChangeDetector changeDetector)
        {
            ResourceId = resourceId;
            InputHandler = inputHandler;
            ChangeDetector = changeDetector;
        }

        internal string ResourceId { get; }
        public IInputHandler InputHandler { get; }
        public IChangeDetector ChangeDetector { get; }
    }

    // TODO: Implement IDisposable.
    public sealed class ScreenFactory : IScreenFactory
    {
        private readonly object _printLock = new object();
        private readonly ITextGenerator _textGenerator;
        private readonly IConnectionManager _connectionManager;
        private readonly ICharactersClient _charactersClient;
        private readonly IOutput _output;

        // TODO: Refactor to be generic.
        private readonly IPrinter<MainMenuState> _mainMenuPrinter;
        private readonly IPrinter<CharacterCreationState> _characterCreationPrinter;

        private readonly Dictionary<string, IEnumerable<IDisposable>> _resources
            = new Dictionary<string, IEnumerable<IDisposable>>();

        public ScreenFactory(
            ITextGenerator textGenerator,
            IConnectionManager connectionManager,
            ICharactersClient charactersClient,
            IOutput output,
            IPrinter<MainMenuState> mainMenuPrinter,
            IPrinter<CharacterCreationState> characterCreationPrinter)
        {
            _textGenerator = textGenerator;
            _connectionManager = connectionManager;
            _charactersClient = charactersClient;
            _output = output;
            _mainMenuPrinter = mainMenuPrinter;
            _characterCreationPrinter = characterCreationPrinter;

            // Create this only after initializing everything inside ScreenFactory.
            ScreenNavigation = new ScreenNavigation(this);

            Task.Run(() => ScreenNavigation.Screen = GameScreen.MainMenu);
        }

        public IScreenNavigation ScreenNavigation { get; }

        public IScreen OpenScreen(GameScreen screenType) => screenType switch
        {
            GameScreen.MainMenu => CreateMainMenuScreen(),
            GameScreen.CharacterCreation => CreateCharacterCreationScreen(),
            GameScreen.Exit => null!,
            _ => throw new InvalidOperationException("Unknown screen type.")
        };

        public void CloseScreen(IScreen screen)
        {
            foreach (var disposable in _resources[((Screen)screen).ResourceId])
            {
                disposable.Dispose();
            }

            _resources.Remove(((Screen)screen).ResourceId);
        }

        private IScreen CreateMainMenuScreen()
        {
            var typerPool = new UniqueTyperPool(_textGenerator);
            var componentPool = new ComponentPool(typerPool);
            var state = new MainMenuState(typerPool, componentPool);

            var mainMenuInputHandler = new MainMenuInputHandler(
                typerPool,
                componentPool,
                state,
                ScreenNavigation,
                _connectionManager);

            var mainMenuStateManager = new MainMenuStateManager(
                _charactersClient,
                state);

            var printer = new StatePrinter<MainMenuState>(
                _printLock,
                ScreenNavigation,
                _output,
                mainMenuStateManager.StateObservable,
                _mainMenuPrinter,
                GameScreen.MainMenu);

            var resourceId = Guid.NewGuid().ToString();
            var resources = new IDisposable[] { mainMenuStateManager, printer };

            _resources.Add(resourceId, resources);

            return new Screen(resourceId, mainMenuInputHandler, mainMenuStateManager);
        }

        private IScreen CreateCharacterCreationScreen()
        {
            var typerPool = new UniqueTyperPool(_textGenerator);
            var componentPool = new ComponentPool(typerPool);
            var state = new CharacterCreationState(typerPool, componentPool);

            var characterCreationInputHandler = new CharacterCreationInputHandler(
                typerPool,
                componentPool,
                state,
                ScreenNavigation,
                _charactersClient);

            var characterCreationStateManager = new CharacterCreationStateManager(state);

            var printer = new StatePrinter<CharacterCreationState>(
                _printLock,
                ScreenNavigation,
                _output,
                characterCreationStateManager.StateObservable,
                _characterCreationPrinter,
                GameScreen.CharacterCreation);

            var resourceId = Guid.NewGuid().ToString();
            var resources = new IDisposable[] { characterCreationStateManager, printer };

            _resources.Add(resourceId, resources);

            return new Screen(resourceId, characterCreationInputHandler, characterCreationStateManager);
        }
    }

    public sealed class ScreenNavigation : IScreenNavigation, IScreenProvider
    {
        private readonly BehaviorSubject<GameScreen> _screenSubject;
        //private readonly DialogScreenHandler _dialogScreenHandler;
        private readonly IScreenFactory _screenFactory;

        public ModalModule ActiveModalModule { get; set; }
        public ModalModule BackgroundModalModule { get; set; }
        public IScreenHandler? Dialog { get; set; }

        public GameScreen Screen
        {
            get => _screenSubject.Value;
            set
            {
                if (Screen == value && CurrentScreen != null /* so that initialization works */)
                    return;

                //CurrentScreen = null;
                //_screenSubject.OnNext(value);
                if (CurrentScreen != null)
                    _screenFactory.CloseScreen(CurrentScreen);

                CurrentScreen = _screenFactory.OpenScreen(value);
                _screenSubject.OnNext(value);
            }
        }

        public ScreenNavigation(
            //DialogScreenHandler dialogScreenHandler,
            IScreenFactory screenFactory)
        {
            //_dialogScreenHandler = dialogScreenHandler;
            _screenSubject = new BehaviorSubject<GameScreen>(GameScreen.MainMenu);
            _screenFactory = screenFactory;
        }

        public IObservable<GameScreen> ScreenObservable => _screenSubject
            .Where(screen => screen != GameScreen.Exit);

        public IScreen? CurrentScreen { get; private set; }

        public void OpenDialog(string text, Action ok, Action cancel)
        {
            if (Dialog != null)
                return; // Do not override existing dialog.

            /*_dialogScreenHandler.Initialize(text, OkAction, CancelAction);
            Dialog = _dialogScreenHandler;*/

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

        // TODO: Do not return null screen BUT return LOADING screen if it's null.
        public IScreen GetCurrentScreen() => CurrentScreen!;
    }
}
