using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Handling
{
    public sealed class ConnectionHandler : IConnectionHandler
    {
        private readonly ILogger<ConnectionHandler> _logger;
        private readonly IConnectionInitializer _connectionInitializer;
        private readonly IConnectedClientStore _connectedClients;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IUpdateDetector _updateDetector;
        private readonly IUpdater _updater;

        public ConnectionHandler(
            ILogger<ConnectionHandler> logger,
            IConnectionInitializer connectionInitializer,
            IConnectedClientStore connectedClients,
            IMessageDispatcher messageDispatcher,
            IUpdateDetector updateDetector,
            IUpdater updater)
        {
            _logger = logger;
            _connectionInitializer = connectionInitializer;
            _connectedClients = connectedClients;
            _messageDispatcher = messageDispatcher;
            _updateDetector = updateDetector;
            _updater = updater;
        }

        public async Task HandleAsync(IConnection connection, CancellationToken cancellationToken)
        {
            var connectedClient = await _connectionInitializer.ConnectAsync(connection, cancellationToken).ConfigureAwait(false);
            _connectedClients.Add(connectedClient);

            while (_connectedClients.IsClientConnected(connectedClient.ClientId))
            {
                var message = await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                await DispatchAndUpdateAsync(connectedClient, message, cancellationToken).ConfigureAwait(false);
            }
        }

        private async ValueTask DispatchAndUpdateAsync(ConnectedClient sender, object message, CancellationToken cancellationToken)
        {
            ValueTask? disconnecting = null;

            try
            {
                // The message propagates to all the handlers and waits for them to finish.
                await _messageDispatcher.DispatchAsync(sender, message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"There was an error when handling {message.GetType().Name} message for {sender.ClientId} client ID.");

#pragma warning disable CA2012 // We store ValueTask in a variable to await it later once.
                disconnecting = sender.Connection.SendAsync(new Disconnected($"Error when handling {message.GetType().Name} message."), cancellationToken);
#pragma warning restore CA2012

                _connectedClients.Remove(sender.ClientId);

                throw;
            }
            finally
            {
                // Do not lose the exception from catch block.
                try
                {
                    _updateDetector.MarkForUpdate(sender.Group);

                    var clientsThatNeedUpdate = _connectedClients.FindInGroups(_updateDetector.PopMarked()).ToList();

                    await AsyncHelpers.WhenAll(clientsThatNeedUpdate
                        .Select(c => _updater.SendUpdateAsync(c, cancellationToken))).ConfigureAwait(false);

                    if (disconnecting != null)
                    {
                        await disconnecting.Value.ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Error during cleanup & update sending after message handling.");

                    if (disconnecting == null)
                        throw; // No exception was thrown in catch block - safe to throw here.
                }
            }
        }
    }
}
