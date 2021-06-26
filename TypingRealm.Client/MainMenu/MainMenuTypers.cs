using System;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuTypers
    {
        private readonly ITyperPool _typerPool;
        private readonly ComponentPool _componentPool;

        public MainMenuTypers(
            ITyperPool typerPool,
            ComponentPool componentPool)
        {
            _typerPool = typerPool;
            _componentPool = componentPool;

            TestInput = _componentPool.MakeInputComponent();
            _typerPool.MakeUniqueTyper("exit");
            _typerPool.MakeUniqueTyper("create-character");
        }

        public Typer Exit => _typerPool.GetByKey("exit") ?? throw new InvalidOperationException();
        public Typer CreateCharacter => _typerPool.GetByKey("create-character") ?? throw new InvalidOperationException();
        public InputComponent TestInput { get; }

        public string? GetCharacterIdFor(Typer typer)
        {
            var key = _typerPool.GetKeyFor(typer);
            if (key == null || !key.StartsWith("select-character-"))
                return null;

            return key.Replace("select-character-", string.Empty);
        }

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
