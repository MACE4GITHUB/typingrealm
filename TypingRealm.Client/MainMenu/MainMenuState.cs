using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuState
    {
        private readonly ITyperPool _typerPool;
        private readonly HashSet<SelectCharacter> _characters
            = new HashSet<SelectCharacter>();

        public MainMenuState(ITyperPool typerPool)
        {
            _typerPool = typerPool;

            Exit = _typerPool.MakeUniqueTyper("exit");
            CreateCharacter = _typerPool.MakeUniqueTyper("create-character");
        }

        public Typer Exit { get; }
        public Typer CreateCharacter { get; }
        public IEnumerable<SelectCharacter> Characters => _characters;

        public string? GetCharacterIdFor(Typer typer)
        {
            return Characters.SingleOrDefault(c => c.Typer == typer)?
                .CharacterId;
        }

        public void SyncSelectCharacterTyper(string characterId, string name)
        {
            if (Characters.Any(c => c.CharacterId == characterId))
                return;

            var typer = _typerPool.MakeUniqueTyper($"select-character-{characterId}");
            _characters.Add(new SelectCharacter(characterId, name, typer));
        }

        public void RemoveSelectCharacterTyper(string characterId)
        {
            var character = Characters.SingleOrDefault(c => c.CharacterId == characterId);
            if (character == null)
                return;

            _characters.Remove(character);
            _typerPool.RemoveTyper(character.Typer);
        }

        public Typer GetSelectCharacterTyper(string characterId)
        {
            return Characters.SingleOrDefault(c => c.CharacterId == characterId)?.Typer
                ?? throw new InvalidOperationException();
        }
    }
}
