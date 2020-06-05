using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Movement
{
    public sealed class MovementHandler
        : IMessageHandler<EnterRoad>,
        IMessageHandler<Move>
    {
        private readonly IPlayerRepository _playerRepository;

        public MovementHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public ValueTask HandleAsync(ConnectedClient sender, EnterRoad message, CancellationToken cancellationToken)
        {
            var player = _playerRepository.Find(new PlayerId(sender.ClientId));

            player!.EnterRoad(new RoadId(message.RoadId));

            _playerRepository.Save(player);
            return default;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Move message, CancellationToken cancellationToken)
        {
            var player = _playerRepository.Find(new PlayerId(sender.ClientId));

            player!.Move(message.Distance);

            _playerRepository.Save(player);
            return default;
        }
    }
}
