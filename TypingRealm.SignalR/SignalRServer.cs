using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.SignalR
{
    public sealed class SignalRServer : AsyncManagedDisposable, ISignalRServer
    {
        private readonly ILogger<SignalRServer> _logger;
        private readonly IScopedConnectionHandler _connectionHandler;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<Task> _connectionProcessors = new List<Task>();
        private readonly ISignalRConnectionFactory _signalRConnectionFactory;
        private readonly ConcurrentDictionary<string, SignalRConnectionResource> _notificators
            = new ConcurrentDictionary<string, SignalRConnectionResource>();

        public SignalRServer(
            ILogger<SignalRServer> logger,
            IScopedConnectionHandler connectionHandler,
            ISignalRConnectionFactory signalRConnectionFactory)
        {
            _logger = logger;
            _connectionHandler = connectionHandler;
            _signalRConnectionFactory = signalRConnectionFactory;
        }

        public void NotifyReceived(string connectionId, object message)
        {
            _notificators.TryGetValue(connectionId, out var resource);

            // This happens when server threw an exception but did not disconnect the client, and the client did not disconnect and tries to send more messages.
            if (resource == null)
                throw new InvalidOperationException($"Connection {connectionId} does not exist.");

            resource.Notificator.NotifyReceived(message);
        }

        public void StartHandling(HubCallerContext context, IClientProxy caller)
            => _ = HandleAsync(context, caller);

        public async Task StopHandling(string connectionId)
        {
            if (_notificators.TryGetValue(connectionId, out var resource))
                await resource.CancelAsync().ConfigureAwait(false);
        }

        private async Task HandleAsync(HubCallerContext context, IClientProxy caller)
        {
            string connectionDetails;

            try
            {
                connectionDetails = $"{context.ConnectionId}, {context.UserIdentifier}, {context.User.Identity?.Name}";
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
                var connection = _signalRConnectionFactory.CreateProtobufConnectionForServer(caller, notificator);

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

                // Warning: when exception is thrown inside the task, cancellation token is not canceled (only disposed).
                await resource.CancelAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error happened when creating or handling SignalR connection: {connectionDetails}");
            }
            finally
            {
                // Disconnect the client from the server side.
                context.Abort();

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
