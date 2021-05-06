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
        private readonly IClientToServerMessageMetadataFactory _metadataFactory;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly SemaphoreSlimLock _reconnectLock = new SemaphoreSlimLock();
        private readonly Dictionary<string, Func<object, ValueTask>> _handlers
            = new Dictionary<string, Func<object, ValueTask>>();
        private readonly Dictionary<string, Func<object, string?, ValueTask>> _handlersWithId
            = new Dictionary<string, Func<object, string?, ValueTask>>();

        private static readonly int _reconnectRetryCount = 3;

        private ConnectionResource? _resource;

        public MessageProcessor(
            ILogger<MessageProcessor> logger,
            IClientConnectionFactory connectionFactory,
            IMessageDispatcher dispatcher,
            IProfileTokenProvider profileTokenProvider,
            IClientToServerMessageMetadataFactory metadataFactory,
            IMessageTypeCache messageTypeCache)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _dispatcher = dispatcher;
            _profileTokenProvider = profileTokenProvider;
            _metadataFactory = metadataFactory;
            _messageTypeCache = messageTypeCache;
        }

        public bool IsConnected { get; private set; }

        // This method is used within a lock so it shouldn't wait for lock itself anywhere inside.
        public async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected.");

            var connectionWithDisconnect = await _connectionFactory.ConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            // Need to set it before calling ListenAndDispatchAsync.
            IsConnected = true;

            _resource = new ConnectionResource(
                connectionWithDisconnect,
                cancellationToken);

            Task listening(CancellationToken cancellationToken)
                => ListenAndDispatchAsync();

            _resource.SetListening(listening);
        }

        public ValueTask SendAsync(
            object message,
            CancellationToken cancellationToken)
            => SendAsync(message, null, cancellationToken);

        public async ValueTask SendAsync(
            object message,
            Action<ClientToServerMessageMetadata>? metadataSetter,
            CancellationToken cancellationToken)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected.");

            var resource = GetConnectionResource();

            var metadata = _metadataFactory.CreateFor(message);
            metadataSetter?.Invoke(metadata);

            var reconnectedTimes = 0;
            while (reconnectedTimes < _reconnectRetryCount)
            {
                try
                {
                    await resource.UseCombinedCts(ct => resource.Connection.SendAsync(message, metadata, ct), cancellationToken)
                        .ConfigureAwait(false);

                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while trying to send a message.");
                    reconnectedTimes++;
                    IsConnected = false;

                    if (reconnectedTimes == _reconnectRetryCount)
                        throw;

                    await ReconnectAsync()
                        .ConfigureAwait(false);
                }
            }
        }

        public async ValueTask<TResponse> SendQueryAsync<TResponse>(object message, CancellationToken cancellationToken)
            where TResponse : class
        {
            TResponse? response = null;
            string? subscriptionId = null;

            try
            {
                // We're passing original token here because SendAsync method will have passed combined one.
                await SendAsync(message, metadata =>
                {
                    ValueTask Handler(TResponse result)
                    {
                        response = result;
                        return default;
                    }

                    subscriptionId = SubscribeWithMessageId<TResponse>(Handler, metadata.MessageId);

                    metadata.ResponseMessageTypeId = _messageTypeCache.GetTypeId(typeof(TResponse));
                }, cancellationToken)
                    .ConfigureAwait(false);

                var i = 0;
                while (response == null)
                {
                    // TODO: Pass combined token here.
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                    i++;

                    // TODO: Improve this.
                    if (i > 300)
                        throw new InvalidOperationException("Could not receive response in time (timeout).");
                }
            }
            finally
            {
                Unsubscribe(subscriptionId!);
            }

            return response!;
        }

        // I Cannot use SubscribeWithId method because it works only after initial connection has been established.
        // But we need to have acknowledgement on the level of all messages (like Authentication, that are used during connection stage).
        public async ValueTask SendAcknowledgedAsync(object message, CancellationToken cancellationToken)
        {
            // TODO: Re-do this using CTS and Task.Delay(cts) instead of isAcknowledged.
            var isAcknowledged = false;
            string? subscriptionId = null;

            try
            {
                await SendAsync(message, metadata =>
                {
                    ValueTask Handler(AcknowledgeReceived acknowledgeReceived)
                    {
                        if (acknowledgeReceived.MessageId == metadata.MessageId)
                            isAcknowledged = true;

                        return default;
                    }

                    subscriptionId = SubscribeWithMessageId<AcknowledgeReceived>(Handler, metadata.MessageId);

                    metadata.RequireAcknowledgement = true;
                }, cancellationToken)
                    .ConfigureAwait(false);

                var i = 0;
                while (!isAcknowledged)
                {
                    // TODO: Pass combined token here.
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                    i++;

                    // TODO: Improve this.
                    if (i > 300)
                        throw new InvalidOperationException("Could not receive response in time (timeout). Acknowledgement is not received.");
                }
            }
            finally
            {
                Unsubscribe(subscriptionId!);
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
            var resource = _resource;

            if (resource == null)
                return;

            await resource.DisposeAsync().ConfigureAwait(false);
        }

        // Here comes original cancellation token.
        private async Task ListenAndDispatchAsync()
        {
            var resource = GetConnectionResource();

            while (IsConnected)
            {
                try
                {
                    var message = await resource!.Connection.ReceiveAsync(resource.CombinedCts.Token)
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
                            _ = SendAsync(new Authenticate(token), resource.CombinedCts.Token);
                            break;
                    }

                    await _dispatcher.DispatchAsync(message, resource.CombinedCts.Token)
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
                    _ = ReconnectAsync().ConfigureAwait(false);
                    return;
                }
            }
        }

        private async Task ReconnectAsync()
        {
            if (IsConnected)
                return;

            var resource = GetConnectionResource();

            await using var _ = await _reconnectLock.UseWaitAsync(resource.OriginalCancellationToken)
                .ConfigureAwait(false);

            if (IsConnected)
                return;

            await resource.DisposeAsync()
                .ConfigureAwait(false);

            await ConnectAsync(resource.OriginalCancellationToken).ConfigureAwait(false);
        }

        private ConnectionResource GetConnectionResource()
        {
            var resource = _resource;

            if (resource is null)
                throw new InvalidOperationException("Connection resource has been erased. Cannot continue.");

            return resource;
        }
    }
}
