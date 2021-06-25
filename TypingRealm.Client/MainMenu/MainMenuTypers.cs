using System;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuTypers
    {
        private readonly ITyperPool _typerPool;

        public MainMenuTypers(ITyperPool typerPool)
        {
            _typerPool = typerPool;

            _typerPool.MakeUniqueTyper("exit");
            _typerPool.MakeUniqueTyper("create-character");
        }

        public Typer Exit => _typerPool.GetByKey("exit") ?? throw new InvalidOperationException();
        public Typer CreateCharacter => _typerPool.GetByKey("create-character") ?? throw new InvalidOperationException();

        public void SyncSelectCharacterTyper(string characterId)
        {
            if (_typerPool.GetByKey($"select-character-{characterId}") != null)
                return;

            _typerPool.MakeUniqueTyper($"select-character-{characterId}");
        }

        public void RemoveSelectCharacterTyper(string characterId)
        {
            _typerPool.RemoveTyper($"select-character-{characterId}");
        }

        public Typer GetSelectCharacterTyper(string characterId)
        {
            return _typerPool.GetByKey($"select-character-{characterId}")
                ?? throw new InvalidOperationException();
        }
    }
}
