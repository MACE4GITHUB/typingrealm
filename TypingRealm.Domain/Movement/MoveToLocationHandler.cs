using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Movement
{
    public sealed class MoveToLocationHandler : IMessageHandler<MoveToLocation>
    {
        private readonly IPlayerRepository _players;

        public MoveToLocationHandler(IPlayerRepository players)
        {
            _players = players;
        }

        public ValueTask HandleAsync(ConnectedClient sender, MoveToLocation message, CancellationToken cancellationToken)
        {
            var player = _players.GetByClientId(sender.ClientId);
            player.MoveTo(message.LocationId);
            _players.Save(sender.ClientId, player);

            sender.Group = player.GetUniqueLocation();

            return default;
        }
    }
}
