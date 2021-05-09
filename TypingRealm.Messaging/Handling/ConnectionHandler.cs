using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Handling
{
    public sealed class ConnectionHandler : IConnectionHandler
    {
        private readonly ILogger<ConnectionHandler> _logger;
        private readonly IConnectionInitializer _connectionInitializer;
        private readonly IConnectedClientStore _connectedClients;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IUpdateDetector _updateDetector;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly IUpdater _updater;

        public ConnectionHandler(
            ILogger<ConnectionHandler> logger,
            IConnectionInitializer connectionInitializer,
            IConnectedClientStore connectedClients,
            IMessageDispatcher messageDispatcher,
            IQueryDispatcher queryDispatcher,
            IUpdateDetector updateDetector,
            IMessageTypeCache messageTypeCache,
            IUpdater updater)
        {
            _logger = logger;
            _connectionInitializer = connectionInitializer;
            _connectedClients = connectedClients;
            _messageDispatcher = messageDispatcher;
            _queryDispatcher = queryDispatcher;
            _updateDetector = updateDetector;
            _messageTypeCache = messageTypeCache;
            _updater = updater;
        }

        public async Task HandleAsync(IConnection connection, CancellationToken cancellationToken)
        {
            ConnectedClient connectedClient;

            connection = connection.WithReceiveAcknowledgement();
            var unwrapperConnection = new ServerMessageUnwrapperConnection(connection);

            try
            {
                connectedClient = await _connectionInitializer.ConnectAsync(unwrapperConnection, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                try
                {
                    await connection.SendAsync(new Disconnected($"Error during connection initialization."), cancellationToken)
                        .ConfigureAwait(false);
                } catch { }

                throw;
            }

            _connectedClients.Add(connectedClient);
            if (!_connectedClients.IsClientConnected(connectedClient.ClientId))
                throw new InvalidOperationException("Client was not added correctly.");

            // TODO: Unit test all the logic about idempotency.
            var idempotencyKeys = new Dictionary<string, DateTime>();

            // TODO: Send only to groups that were specified in Metadata from the client (if they were sent).
            await TrySendPendingUpdates(connectedClient.Groups, cancellationToken).ConfigureAwait(false);
            while (_connectedClients.IsClientConnected(connectedClient.ClientId))
            {
                ClientToServerMessageMetadata? metadata = null;

                try
                {
                    var message = await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                    if (message is not ClientToServerMessageWithMetadata messageWithMetadata)
                        throw new InvalidOperationException($"Message is not of {typeof(ClientToServerMessageWithMetadata).Name} type.");

                    metadata = messageWithMetadata.Metadata;

                    if (messageWithMetadata.Metadata.MessageId != null && idempotencyKeys.ContainsKey(messageWithMetadata.Metadata.MessageId))
                    {
                        _logger.LogDebug($"Message with id {messageWithMetadata.Metadata.MessageId} has already been handled. Skipping duplicate (idempotency).");
                        continue;
                    }

                    await DispatchMessageAsync(connectedClient, messageWithMetadata.Message, cancellationToken).ConfigureAwait(false);

                    // TODO: Unit test this.
                    if (messageWithMetadata.Metadata.ResponseMessageTypeId != null)
                    {
                        // TODO: Send query response in background, do not block connection handling.
                        var responseType = _messageTypeCache.GetTypeById(messageWithMetadata.Metadata.ResponseMessageTypeId);
                        var response = await _queryDispatcher.DispatchAsync(connectedClient, messageWithMetadata.Message, responseType, cancellationToken)
                            .ConfigureAwait(false);

                        // TODO: Do this also in background.
                        await connectedClient.Connection.SendAsync(response, new ServerToClientMessageMetadata
                        {
                            RequestMessageId = messageWithMetadata.Metadata.MessageId
                        }, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    // If everything was dispatched successfully:
                    if (messageWithMetadata.Metadata.MessageId != null && !idempotencyKeys.ContainsKey(messageWithMetadata.Metadata.MessageId))
                        idempotencyKeys.Add(messageWithMetadata.Metadata.MessageId, DateTime.UtcNow);

                    foreach (var item in idempotencyKeys.Where(x => x.Value < DateTime.UtcNow - TimeSpan.FromMinutes(1)))
                    {
                        idempotencyKeys.Remove(item.Key);
                    }

                    if (messageWithMetadata.Metadata.RequireAcknowledgement)
                    {
                        // !!!
                        // I need to get acknowledgement from Authentication message and it's not possible
                        // to send it here since we are still in ConnectionInitializer at that time.
                        // So it definitely needs to be done as Connection decorator.

                        // TODO: Send acknowledgement (it's already being sent, but in Connection decorator instead of after handling the code).
                        // Send AcknowledgeHandled instead of AcknowledgeReceived (probably no need to send both, just rename this message).
                    }
                }
                catch
                {
                    _connectedClients.Remove(connectedClient.ClientId);
                    throw; // If you delete this line and have ncrunch, your PC will die.
                }
                finally
                {
                    var groups = connectedClient.Groups;

                    // TODO: Unit test this (affected groups).
                    if (metadata?.AffectedGroups != null)
                        groups = groups.Where(group => metadata.AffectedGroups.Contains(group));

                    await TrySendPendingUpdates(connectedClient.Groups, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async ValueTask DispatchMessageAsync(ConnectedClient sender, object message, CancellationToken cancellationToken)
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
                    if (disconnecting != null)
                    {
                        await disconnecting.Value.ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Error during sending Disconnected message after message handling failed.");
                }
            }
        }

        private async ValueTask TrySendPendingUpdates(IEnumerable<string> groups, CancellationToken cancellationToken)
        {
            try
            {
                _updateDetector.MarkForUpdate(groups);

                var clientsThatNeedUpdate = _connectedClients.FindInGroups(_updateDetector.PopMarked()).ToList();

                await AsyncHelpers.WhenAll(clientsThatNeedUpdate
                    .Select(c => _updater.SendUpdateAsync(c, cancellationToken))).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // TODO: Disconnect player if update was unsuccessful. Currently it silently continues working (investigate).
                _logger.LogError(exception, $"Error during sending pending updates.");
            }
        }
    }
}
