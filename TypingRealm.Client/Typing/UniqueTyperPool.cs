using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Client.Typing
{
    public sealed class UniqueTyperPool : ITyperPool
    {
        private readonly ITextGenerator _textGenerator;
        private readonly Dictionary<char, Typer> _typers
            = new Dictionary<char, Typer>();

        public UniqueTyperPool(ITextGenerator textGenerator)
        {
            _textGenerator = textGenerator;
        }

        public Typer? GetTyper(char firstLetter)
        {
            if (_typers.TryGetValue(firstLetter, out var typer))
                return typer;

            return null;
        }

        public Typer MakeUniqueTyper()
        {
            var phrase = GetUniquePhrase();

            var typer = new Typer(phrase);
            _typers.Add(phrase[0], typer);

            return typer;
        }

        public void ResetUniqueTyper(Typer typer)
        {
            var phrase = GetUniquePhrase();

            typer.Reset(phrase);
            _typers.Remove(_typers.Single(x => x.Value == typer).Key);
            _typers.Add(phrase[0], typer);
        }

        public void RemoveTyper(Typer typer)
        {
            _typers.Remove(_typers.Single(x => x.Value == typer).Key);
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
