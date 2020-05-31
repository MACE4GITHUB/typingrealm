using System.Linq;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Domain
{
    public sealed class UpdateFactory : IUpdateFactory
    {
        private readonly IPlayerRepository _playerRepository;

        public UpdateFactory(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public object GetUpdateFor(string clientId)
        {
            var player = _playerRepository.GetByClientId(clientId);
            var visiblePlayers = _playerRepository.GetPlayersVisibleTo(player.Id)
                .Select(x => x.Id);

            return new Update(player.LocationId, visiblePlayers);
        }
    }
}
