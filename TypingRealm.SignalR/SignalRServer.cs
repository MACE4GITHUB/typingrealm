using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.SignalR
{
    public sealed class SignalRServer : AsyncManagedDisposable, ISignalRServer
    {
        private readonly ILogger<SignalRServer> _logger;
        private readonly IScopedConnectionHandler _connectionHandler;
        private readonly IJsonConnectionFactory _jsonConnectionFactory;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<Task> _connectionProcessors = new List<Task>();
        private readonly ConcurrentDictionary<string, SignalRConnectionResource> _notificators
            = new ConcurrentDictionary<string, SignalRConnectionResource>();

        public SignalRServer(
            ILogger<SignalRServer> logger,
            IScopedConnectionHandler connectionHandler,
            IJsonConnectionFactory jsonConnectionFactory)
        {
            _logger = logger;
            _connectionHandler = connectionHandler;
            _jsonConnectionFactory = jsonConnectionFactory;
        }

        public void NotifyReceived(string connectionId, object message)
        {
            _notificators.TryGetValue(connectionId, out var resource);
            if (resource == null)
                throw new InvalidOperationException($"Connection {connectionId} does not exist.");

            resource.Notificator.NotifyReceived(message);
        }

        public void StartHandling(HubCallerContext context, IClientProxy caller)
            => _ = HandleAsync(context, caller);

        public async Task StopHandling(string connectionId)
        {
            if (_notificators.TryGetValue(connectionId, out var resource))
                await resource.Cancel().ConfigureAwait(false);
        }

        private async Task HandleAsync(HubCallerContext context, IClientProxy caller)
        {
            string connectionDetails;

            try
            {
                connectionDetails = $"{context.ConnectionId}, {context.UserIdentifier}, {context.User.Identity.Name}";
            }
            catch (Exception exception)
            {
                connectionDetails = "Failed to get details";
                _logger.LogError(exception, "Failed to get connection details.");
            }

            try
            {
                using var localCts = new CancellationTokenSource();
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cts.Token, localCts.Token);

                var notificator = new Notificator();
                var connection = new SignalRMessageSender(caller)
                    .WithNotificator(notificator)
                    .WithJson(_jsonConnectionFactory);

                var task = _connectionHandler
                    .HandleAsync(connection, combinedCts.Token)
                    .HandleCancellationAsync(exception =>
                    {
                        _logger.LogDebug(exception, $"Cancellation request received for client: {connectionDetails}");
                    })
                    .HandleExceptionAsync<Exception>(exception =>
                    {
                        _logger.LogError(exception, $"Error happened while handling SignalR connection: {connectionDetails}");
                    });

                var resource = new SignalRConnectionResource(
                    notificator,
                    async () =>
                    {
                        localCts.Cancel();
                        await task.ConfigureAwait(false);
                    });

                _notificators.TryAdd(context.ConnectionId, resource);

                _connectionProcessors.Add(task);
                _connectionProcessors.RemoveAll(t => t.IsCompleted);

                await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error happened when creating SignalR connection: {connectionDetails}");
            }
            finally
            {
                _notificators.TryRemove(context.ConnectionId, out _);
            }
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            _cts.Cancel();

            await Task.WhenAll(_connectionProcessors).ConfigureAwait(false);

            _cts.Dispose();
        }
    }
}
