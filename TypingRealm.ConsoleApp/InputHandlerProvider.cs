using System;
using System.Collections.Generic;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.MainMenu;

namespace TypingRealm.ConsoleApp
{
    public sealed class InputHandlerProvider : IScreenProvider
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IDictionary<GameScreen, IInputHandler> _screenHandlers;
        private readonly IDictionary<GameScreen, IChangeDetector> _changeDetectors;

        public InputHandlerProvider(
            IScreenNavigation screenNavigation,
            IDictionary<GameScreen, IInputHandler> screenHandlers,
            IDictionary<GameScreen, IChangeDetector> changeDetectors)
        {
            _screenNavigation = screenNavigation;
            _screenHandlers = screenHandlers;
            _changeDetectors = changeDetectors;
        }

        public IChangeDetector GetCurrentChangeDetector()
        {
            return _changeDetectors[_screenNavigation.Screen];
        }

        public IInputHandler GetCurrentInputHandler()
        {
            if (_screenNavigation.Dialog != null)
                return _screenNavigation.Dialog;

            if (_screenNavigation.ActiveModalModule != ModalModule.None)
            {
                throw new NotImplementedException();
            }

            return _screenHandlers[_screenNavigation.Screen];
        }
    }
}
