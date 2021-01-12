using System.Collections.Generic;
using System.Linq;
using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.Typing
{
    public abstract class MultiTyperInputHandler : IInputHandler
    {
        private readonly ITextGenerator _textGenerator;
        private readonly Dictionary<char, Typer> _typers
            = new Dictionary<char, Typer>();

        private Typer? _focusedTyper;

        protected MultiTyperInputHandler(ITextGenerator textGenerator)
        {
            _textGenerator = textGenerator;
        }

        public void Type(char character)
        {
            if (_focusedTyper == null)
            {
                if (!_typers.ContainsKey(character))
                    return;

                _focusedTyper = _typers[character];
            }

            _focusedTyper.Type(character);

            if (_focusedTyper.IsFinishedTyping)
            {
                ResetUniqueTyper(_focusedTyper);

                OnTyped(_focusedTyper);
                _focusedTyper = null;
            }
        }

        public void Backspace()
        {
            if (_focusedTyper == null)
                return;

            _focusedTyper.Backspace();

            if (!_focusedTyper.IsStartedTyping)
                _focusedTyper = null;
        }

        public void Escape()
        {
            if (_focusedTyper != null)
            {
                _focusedTyper.Reset();
                _focusedTyper = null;
            }
        }

        public virtual void Tab() { }

        protected abstract void OnTyped(Typer typer);

        protected Typer MakeUniqueTyper()
        {
            var phrase = GetUniquePhrase();

            var typer = new Typer(phrase);
            _typers.Add(phrase[0], typer);

            return typer;
        }

        private void ResetUniqueTyper(Typer typer)
        {
            var phrase = GetUniquePhrase();

            typer.Reset(phrase);
            _typers.Remove(_typers.Single(x => x.Value == typer).Key);
            _typers.Add(phrase[0], typer);
        }

        private string GetUniquePhrase()
        {
            var phrase = _textGenerator.GetPhrase();
            while (_typers.ContainsKey(phrase[0]))
                phrase = _textGenerator.GetPhrase();

            return phrase;
        }
    }
}
