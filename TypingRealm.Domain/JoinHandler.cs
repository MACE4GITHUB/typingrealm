using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain
{
    public sealed class JoinHandler : IMessageHandler<Join>
    {
        private readonly IPlayerRepository _playerRepository;

        public JoinHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Join message, CancellationToken cancellationToken)
        {
            var playerId = Guid.NewGuid().ToString();
            var player = new Player(playerId, message.Name);

            _playerRepository.Save(sender.ClientId, player);
            return default;
        }
    }
}
