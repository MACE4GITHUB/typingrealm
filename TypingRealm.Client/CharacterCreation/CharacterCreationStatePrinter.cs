using System;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationStatePrinter : SyncManagedDisposable
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IOutput _output;
        private readonly CharacterCreationScreenStateManager _stateManager;
        private readonly IPrinter<CharacterCreationScreenState> _statePrinter;
        private readonly object _printLock = new object();
        private IDisposable? _subscription;

        public CharacterCreationStatePrinter(
            IScreenNavigation screenNavigation,
            IOutput output,
            CharacterCreationScreenStateManager stateManager,
            IPrinter<CharacterCreationScreenState> statePrinter)
        {
            _screenNavigation = screenNavigation;
            _output = output;
            _stateManager = stateManager;
            _statePrinter = statePrinter;

            Subscribe();
        }

        private void Subscribe()
        {
            ThrowIfDisposed();

            if (_subscription != null)
                throw new InvalidOperationException("Already subscribed.");

            _subscription = _stateManager.StateObservable.Subscribe(state =>
            {
                if (_screenNavigation.Screen != GameScreen.CharacterCreation)
                    return;

                if (state == null)
                {
                    lock (_printLock)
                    {
                        _output.Clear();
                        _output.WriteLine("LOADING...");
                        _output.FinalizeScreen();
                        return;
                    }
                }

                lock (_printLock)
                {
                    _output.Clear();
                    _statePrinter.Print(state);
                    _output.FinalizeScreen();
                }
            });
        }

        protected override void DisposeManagedResources()
        {
            _subscription?.Dispose();
        }
    }
}
