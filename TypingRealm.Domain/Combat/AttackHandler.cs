using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Combat
{
    public sealed class AttackHandler : IMessageHandler<Attack>
    {
        private readonly IPlayerRepository _playerRepository;

        public AttackHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Attack message, CancellationToken cancellationToken)
        {
            var attacked = _playerRepository.Find(new PlayerId(message.PlayerId));
            if (attacked == null)
                throw new InvalidOperationException("Player does not exist.");

            var attacker = _playerRepository.Find(new PlayerId(sender.ClientId));
            if (attacker == null)
                throw new InvalidOperationException("Player does not exist.");

            attacker.Attack(attacked);

            // TODO: Mark specific players for update.
            _playerRepository.Save(attacker);
            _playerRepository.Save(attacked);
            return default;
        }
    }
}
