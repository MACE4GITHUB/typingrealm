using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.SignalR
{
    public sealed class MessageHub : Hub
    {
        private readonly ConcurrentDictionary<string, SignalRConnectionResource> _cache;
        private readonly IScopedConnectionHandler _connectionHandler;
        private readonly IJsonConnectionFactory _jsonConnectionFactory;

        public MessageHub(
            ConcurrentDictionary<string, SignalRConnectionResource> cache,
            IScopedConnectionHandler connectionHandler,
            IJsonConnectionFactory jsonConnectionFactory)
        {
            _cache = cache;
            _connectionHandler = connectionHandler;
            _jsonConnectionFactory = jsonConnectionFactory;
        }

        public void Send(JsonSerializedMessage message)
        {
            _cache[Context.ConnectionId].Notificator.NotifyReceived(message);
        }

        public override Task OnConnectedAsync()
        {
            var cts = new CancellationTokenSource();
            var notificator = new Notificator();
            var connection = new SignalRMessageSender(Clients.Caller)
                .WithNotificator(notificator)
                .WithJson(_jsonConnectionFactory);

            var handlingProcess = _connectionHandler.HandleAsync(connection, cts.Token);

            _cache.TryAdd(Context.ConnectionId, new SignalRConnectionResource(
                notificator, async () =>
                {
                    cts.Cancel();
                    await handlingProcess.ConfigureAwait(false);
                    cts.Dispose();
                }));

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_cache.TryRemove(Context.ConnectionId, out var connectionResource))
                await connectionResource.ReleaseResources().ConfigureAwait(false);

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
