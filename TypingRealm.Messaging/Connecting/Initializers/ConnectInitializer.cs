using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Connecting.Initializers
{
    /// <summary>
    /// Connection initializer that accepts only those connections whose first
    /// message is a valid Connect message with supplied ClientId and/or Group.
    /// </summary>
    public sealed class ConnectInitializer : IConnectionInitializer
    {
        private readonly IUpdateDetector _updateDetector;
        private readonly IEnumerable<IConnectHook> _connectHooks;

        public ConnectInitializer(
            IUpdateDetector updateDetector,
            IEnumerable<IConnectHook> connectHooks)
        {
            _updateDetector = updateDetector;
            _connectHooks = connectHooks;
        }

        /// <summary>
        /// Waits for the first message from the connection as a valid Connect message.
        /// </summary>
        public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
        {
            if (!(await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false) is Connect connect))
            {
                await connection.SendAsync(new Disconnected("First message is not a valid Connect message."), cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("First message is not a valid Connect message.");
            }

            foreach (var connectHook in _connectHooks)
            {
                await connectHook.HandleAsync(connect, cancellationToken).ConfigureAwait(false);
            }

            return new ConnectedClient(connect.ClientId, connection, _updateDetector, connect.Group);
        }
    }
}
