using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Client.Typing
{
    public sealed class UniqueTyperPool : ITyperPool
    {
        private readonly ITextGenerator _textGenerator;
        private readonly Dictionary<string, Typer> _idToTypers
            = new Dictionary<string, Typer>();
        private readonly Dictionary<char, Typer> _firstLetterToTypers
            = new Dictionary<char, Typer>();

        public UniqueTyperPool(ITextGenerator textGenerator)
        {
            _textGenerator = textGenerator;
        }

        public Typer? GetTyper(char firstLetter)
        {
            if (_firstLetterToTypers.TryGetValue(firstLetter, out var typer))
                return typer;

            return null;
        }

        public Typer MakeUniqueTyper(string id)
        {
            var phrase = GetUniquePhrase();

            var typer = new Typer(phrase);

            try
            {
                _firstLetterToTypers.Add(phrase[0], typer);
                _idToTypers.Add(id, typer);
            }
            catch
            {
                _firstLetterToTypers.Remove(phrase[0]);
                _idToTypers.Remove(id);
                throw;
            }

            return typer;
        }

        public void ResetUniqueTyper(Typer typer)
        {
            var phrase = GetUniquePhrase();

            typer.Reset(phrase);
            _firstLetterToTypers.Remove(_firstLetterToTypers.Single(x => x.Value == typer).Key);
            _firstLetterToTypers.Add(phrase[0], typer);
        }

        public void RemoveTyper(Typer typer)
        {
            _firstLetterToTypers.Remove(_firstLetterToTypers.Single(x => x.Value == typer).Key);
            _idToTypers.Remove(_idToTypers.Single(x => x.Value == typer).Key);
        }

        private string GetUniquePhrase()
        {
            var phrase = _textGenerator.GetPhrase();
            while (_firstLetterToTypers.ContainsKey(phrase[0]))
                phrase = _textGenerator.GetPhrase();

            return phrase;
        }

        public Typer? GetById(string id)
        {
            if (!_idToTypers.ContainsKey(id))
                return null;

            return _idToTypers[id];
        }
    }
}
