using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public interface IPlayerFactory
    {
        Player CreateNew(PlayerName name);
    }

    public sealed class PlayerFactory : IPlayerFactory
    {
        private readonly ILocationStore _locationStore;
        private readonly IPlayerRepository _playerRepository;

        public PlayerFactory(ILocationStore locationStore, IPlayerRepository playerRepository)
        {
            _locationStore = locationStore;
            _playerRepository = playerRepository;
        }

        public Player CreateNew(PlayerName name)
        {
            var nextId = _playerRepository.NextId();
            var startingLocationId = _locationStore.GetStartingLocationId();

            return new Player(nextId, name, startingLocationId, _locationStore);
        }
    }
}
