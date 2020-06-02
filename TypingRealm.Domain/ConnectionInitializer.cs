using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Domain
{
    /// <summary>
    /// Accepts only those connections whose first message is a valid
    /// <see cref="Join"/> message with supplied PlayerId.
    /// </summary>
    public sealed class ConnectionInitializer : IConnectionInitializer
    {
        private readonly IUpdateDetector _updateDetector;
        private readonly IPlayerRepository _playerRepository;

        public ConnectionInitializer(
            IUpdateDetector updateDetector,
            IPlayerRepository playerRepository)
        {
            _updateDetector = updateDetector;
            _playerRepository = playerRepository;
        }

        public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
