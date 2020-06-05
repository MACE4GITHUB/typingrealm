using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Combat
{
    public sealed class SurrenderHandler : IMessageHandler<Surrender>
    {
        private readonly IPlayerRepository _playerRepository;

        public SurrenderHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Surrender message, CancellationToken cancellationToken)
        {
            var player = _playerRepository.Find(new PlayerId(sender.ClientId));
            if (player == null)
                throw new InvalidOperationException("Player does not exist.");

            player.Surrender(_playerRepository);
            //_playerRepository.Save(player); // Currently player saves itself in passed repository.
            return default;
        }
    }
}
