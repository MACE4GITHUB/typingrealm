using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Authentication;
using TypingRealm.Messaging.Client.Handling;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Messaging.Client
{
    // TODO: Introduce heartbeat and heartbeat timeout when we try to reconnect after no reply.
    public sealed class MessageProcessor : AsyncManagedDisposable, IMessageSender
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IClientConnectionFactory _connectionFactory;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IProfileTokenProvider _profileTokenProvider;
        private readonly IMessageIdFactory _messageIdFactory;
        private readonly IClientToServerMessageMetadataFactory _metadataFactory;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly SemaphoreSlimLock _lock = new SemaphoreSlimLock();
        private readonly Dictionary<string, Func<object, ValueTask>> _handlers
            = new Dictionary<string, Func<object, ValueTask>>();
        private readonly Dictionary<string, Func<object, string?, ValueTask>> _handlersWithId
            = new Dictionary<string, Func<object, string?, ValueTask>>();

        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _combinedCts;
        private ConnectionResource? _connectionResource;
        private Task? _listening;

        public MessageProcessor(
            ILogger<MessageProcessor> logger,
            IClientConnectionFactory connectionFactory,
            IMessageDispatcher dispatcher,
            IProfileTokenProvider profileTokenProvider,
            IMessageIdFactory messageIdFactory,
            IClientToServerMessageMetadataFactory metadataFactory,
            IMessageTypeCache messageTypeCache)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _dispatcher = dispatcher;
            _profileTokenProvider = profileTokenProvider;
            _messageIdFactory = messageIdFactory;
            _metadataFactory = metadataFactory;
            _messageTypeCache = messageTypeCache;
        }

        public bool IsConnected { get; private set; }

        // This method is used within a lock so it shouldn't wait for lock itself anywhere inside.
        public async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected.");

            var connectionResource = await _connectionFactory.ConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            _cts = new CancellationTokenSource();
            _combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            _connectionResource = connectionResource;
            IsConnected = true;

            _listening = ListenAndDispatchAsync(cancellationToken);
        }

        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
                return;

            await using var _ = await _lock.UseWaitAsync(cancellationToken)
                .ConfigureAwait(false);

            if (IsConnected)
                return;

            _cts!.Cancel();
            await _listening!.ConfigureAwait(false); // TODO: This might potentially throw.
            _cts.Dispose();
            _combinedCts!.Dispose();
            await _connectionResource!.DisconnectAsync().ConfigureAwait(false);

            await ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected.");

            try
            {
                await _connectionResource!.Connection.SendAsync(message, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // TODO: ReSend a message if reconnection was successful, otherwise throw an exception.
                // Make sure you generate message ID here so that resending message is idempotent.
                _logger.LogError(exception, "Error while trying to send a message.");

                await ReconnectAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async ValueTask<TResponse> SendQueryAsync<TResponse>(object message, CancellationToken cancellationToken)
            where TResponse : class
        {
            var metadata = _metadataFactory.CreateFor(message);
            metadata.ResponseMessageTypeId = _messageTypeCache.GetTypeId(typeof(TResponse));

            var subscriptionId = SubscribeWithMessageId<TResponse>(Handler, metadata.MessageId);
            TResponse? response = null;

            ValueTask Handler(TResponse result)
            {
                response = result;
                return default;
            }

            try
            {
                // TODO: Do not duplicate this logic (try/catch logic) with Send() method.
                await _connectionResource!.Connection.SendAsync(message, metadata, cancellationToken).ConfigureAwait(false);

                var i = 0;
                while (response == null)
                {
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                    i++;

                    if (i > 300)
                        throw new InvalidOperationException("Could not receive response in time (timeout).");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while trying to send a message.");

                throw;
                /*await ReconnectAsync(cancellationToken)
                    .ConfigureAwait(false);*/

                // TODO: throw or return or smth. Do not return null response!
            }
            finally
            {
                Unsubscribe(subscriptionId);
            }

            return response!;
        }

        // I Cannot use SubscribeWithId method because it works only after initial connection has been established.
        // But we need to have acknowledgement on the level of all messages (like Authentication, that are used during connection stage).
        public async ValueTask SendAcknowledgedAsync(object message, CancellationToken cancellationToken)
        {
            var isAcknowledged = false;
            var metadata = _metadataFactory.CreateFor(message);
            metadata.RequireAcknowledgement = true;

            // TODO: Re-do this using CTS and Task.Delay(cts) instead of isAcknowledged.
            ValueTask Handler(AcknowledgeReceived acknowledgeReceived)
            {
                if (acknowledgeReceived.MessageId == metadata.MessageId)
                    isAcknowledged = true;

                return default;
            }

            var subscriptionId = SubscribeWithMessageId<AcknowledgeReceived>(Handler, metadata.MessageId);

            try
            {
                // TODO: Do not duplicate this logic (try/catch logic) with Send() method.
                await _connectionResource!.Connection.SendAsync(message, metadata, cancellationToken).ConfigureAwait(false);

                var i = 0;
                while (!isAcknowledged)
                {
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                    i++;

                    if (i > 300)
                        throw new InvalidOperationException("Acknowledgement is not received.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while trying to send a message.");

                throw;
                /*await ReconnectAsync(cancellationToken)
                    .ConfigureAwait(false);*/
            }
            finally
            {
                Unsubscribe(subscriptionId);
            }
        }

        public string Subscribe<TMessage>(Func<TMessage, ValueTask> handler)
        {
            var subscriptionId = Guid.NewGuid().ToString();

            _handlers.Add(subscriptionId, message =>
            {
                if (message is TMessage tMessage)
                    return handler(tMessage);

                return default;
            });

            return subscriptionId;
        }

        public string SubscribeWithMessageId<TMessage>(Func<TMessage, ValueTask> handler, string? messageId)
        {
            var subscriptionId = Guid.NewGuid().ToString();

            _handlersWithId.Add(subscriptionId, (message, id) =>
            {
                if (message is TMessage tMessage && id == messageId)
                    return handler(tMessage);

                return default;
            });

            return subscriptionId;
        }

        public void Unsubscribe(string subscriptionId)
        {
            _handlers.Remove(subscriptionId);
            _handlersWithId.Remove(subscriptionId);
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _combinedCts?.Dispose();
            //await _listening?.ConfigureAwait(false); // TODO: Consider doing this.

            var disconnect = _connectionResource?.DisconnectAsync();

            if (disconnect.HasValue)
                await disconnect.Value.ConfigureAwait(false);
        }

        // Here comes original cancellation token.
        private async Task ListenAndDispatchAsync(CancellationToken cancellationToken)
        {
            while (IsConnected)
            {
                try
                {
                    var message = await _connectionResource!.Connection.ReceiveAsync(_combinedCts!.Token)
                        .ConfigureAwait(false);

                    if (message is not ServerToClientMessageWithMetadata messageWithMetadata)
                        throw new InvalidOperationException($"Message is not of {typeof(ServerToClientMessageWithMetadata).Name} type.");

                    message = messageWithMetadata.Message;

                    switch (message)
                    {
                        case Disconnected:
                            IsConnected = false;
                            break;
                        case TokenExpired:
                            var token = await _profileTokenProvider.SignInAsync().ConfigureAwait(false);
                            _ = SendAsync(new Authenticate(token), cancellationToken);
                            break;
                    }

                    await _dispatcher.DispatchAsync(message, _combinedCts!.Token)
                        .ConfigureAwait(false);

                    await AsyncHelpers.WhenAll(_handlers.Values.Select(handler => handler(message)))
                        .ConfigureAwait(false);

                    await AsyncHelpers.WhenAll(_handlersWithId.Values.Select(handler => handler(message, messageWithMetadata.Metadata.RequestMessageId)))
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Receiving message from the server or dispatching it to one of the handlers failed.");

                    // 1. Wait until all Send operations finish (consider canceling local CTS), do not allow any new Send operations to start.
                    IsConnected = false;
                    _ = ReconnectAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}
