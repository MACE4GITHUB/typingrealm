using System;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Interaction
{
    public sealed class DialogModalScreenHandler : MultiTyperInputHandler, IScreenHandler
    {
        private string _text;
        private Typer _ok;
        private Typer _cancel;
        private Action _okAction;
        private Action _cancelAction;

        private readonly IOutput _output;

        public DialogModalScreenHandler(
            ITextGenerator textGenerator,
            IOutput output) : base(textGenerator)
        {
            _output = output;

            _text = null!;
            _ok = null!;
            _cancel = null!;
            _okAction = null!;
            _cancelAction = null!;
        }

        public void Initialize(string text, Action ok, Action cancel)
        {
            _text = text;
            _ok = MakeUniqueTyper();
            _cancel = MakeUniqueTyper();
            _okAction = ok;
            _cancelAction = cancel;
        }

        public void PrintState()
        {
            _output.WriteLine("DIALOG");
            _output.WriteLine(_text);
            _output.WriteLine();
            _output.Write("OK");
            _output.Write(new string(' ', 10));
            _output.WriteLine(_ok);
            _output.Write("CANCEL");
            _output.Write(new string(' ', 10));
            _output.WriteLine(_cancel);
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == _ok)
                _okAction();

            if (typer == _cancel)
                _cancelAction();
        }
    }
}
