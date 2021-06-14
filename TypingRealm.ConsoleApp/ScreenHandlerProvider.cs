using System;
using System.Collections.Generic;
using TypingRealm.Client.Interaction;

namespace TypingRealm.ConsoleApp
{
    public sealed class ScreenHandlerProvider : IScreenHandlerProvider
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly Dictionary<GameScreen, IScreenHandler> _screenHandlers;

        public ScreenHandlerProvider(
            IScreenNavigation screenNavigation,
            Dictionary<GameScreen, IScreenHandler> screenHandlers)
        {
            _screenNavigation = screenNavigation;
            _screenHandlers = screenHandlers;
        }

        public IScreenHandler GetCurrentScreenHandler()
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
