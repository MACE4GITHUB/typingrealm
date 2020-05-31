using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public interface IPlayerFactory
    {
        Player CreateNew(string name);
    }

    public sealed class PlayerFactory : IPlayerFactory
    {
        private readonly ILocationStore _locationStore;

        public PlayerFactory(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public Player CreateNew(string name)
        {
            var startingLocationId = _locationStore.GetStartingLocationId();
            return new Player(PlayerId.New(), name, startingLocationId, _locationStore);
        }
    }
}
