using System;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.Interaction
{
    public sealed class StatePrinter<TState> : SyncManagedDisposable
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IOutput _output;
        private readonly IObservable<TState> _stateObservable;
        private readonly IPrinter<TState> _statePrinter;
        private readonly GameScreen _screen;
        private readonly object _printLock;
        private IDisposable? _subscription;

        public StatePrinter(
            object printLock,
            IScreenNavigation screenNavigation,
            IOutput output,
            IObservable<TState> stateObservable,
            IPrinter<TState> statePrinter,
            GameScreen screen) // TODO: Make sure only one StatePrinter exists per GameScreen.
        {
            _printLock = printLock;
            _screenNavigation = screenNavigation;
            _output = output;
            _stateObservable = stateObservable;
            _statePrinter = statePrinter;
            _screen = screen;

            Subscribe();
        }

        private void Subscribe()
        {
            ThrowIfDisposed();

            if (_subscription != null)
                throw new InvalidOperationException("Already subscribed.");

            _subscription = _stateObservable.Subscribe(state =>
            {
                if (_screenNavigation.Screen != _screen)
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
