using System;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.World
{
    public sealed class WorldStatePrinter : SyncManagedDisposable
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IOutput _output;
        private readonly WorldScreenStateManager _stateManager;
        private readonly IPrinter<WorldScreenState> _statePrinter;
        private readonly object _printLock = new object();
        private IDisposable? _subscription;

        public WorldStatePrinter(
            IScreenNavigation screenNavigation,
            IOutput output,
            WorldScreenStateManager stateManager,
            IPrinter<WorldScreenState> statePrinter)
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
                if (_screenNavigation.Screen != GameScreen.World)
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
