using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.SignalRServer
{
    public sealed class ActiveConnectionCache
    {
        private readonly ConcurrentDictionary<string, ActiveConnection> _activeConnections
            = new ConcurrentDictionary<string, ActiveConnection>();

        public ActiveConnection this[string key] => _activeConnections[key];

        public bool TryAdd(string key, ActiveConnection activeConnection) => _activeConnections.TryAdd(key, activeConnection);

        public bool TryRemove(string key, out ActiveConnection activeConnection) => _activeConnections.TryRemove(key, out activeConnection);
    }

    public sealed class ActiveConnection
    {
        public ActiveConnection(Notificator notificator, Action releaseResources)
        {
            Notificator = notificator;
            ReleaseResources = releaseResources;
        }

        public Notificator Notificator { get; }
        public Action ReleaseResources { get; }
    }

    public sealed class JsonSerializedMessageHub : Hub
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IConnectionHandler _connectionHandler;
        private readonly IJsonConnectionFactory _jsonConnectionFactory;
        private readonly ActiveConnectionCache _cache;

        public JsonSerializedMessageHub(
            IConnectionHandler connectionHandler,
            IJsonConnectionFactory jsonConnectionFactory,
            ActiveConnectionCache cache)
        {
            _connectionHandler = connectionHandler;
            _jsonConnectionFactory = jsonConnectionFactory;
            _cache = cache;
        }

        /// <summary>
        /// This method is called from the client. We get here when the client
        /// sends "Send" message to the <see cref="JsonSerializedMessageHub"/>.
        /// </summary>
        public void Send(JsonSerializedMessage message)
        {
            _cache[Context.ConnectionId].Notificator.NotifyReceived(message);
        }

        public override Task OnConnectedAsync()
        {
            var innerCts = new CancellationTokenSource();
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, innerCts.Token);

            var sendLock = new SemaphoreSlimLock();
            var waitLock = new SemaphoreSlimLock();

            var notificator = new Notificator();
            var connection = new SignalRConnection(Clients.Caller)
                .WithNotificator(notificator)
                .WithJson(_jsonConnectionFactory)
                .WithLocking(sendLock, waitLock);

            var handlingProcess = _connectionHandler.HandleAsync(connection, combinedCts.Token);
            var activeConnection = new ActiveConnection(notificator, async () =>
            {
                innerCts.Cancel();
                await handlingProcess
                    .SwallowCancellationAsync()
                    .ConfigureAwait(false);

                sendLock.Dispose();
                waitLock.Dispose();
                innerCts.Dispose();
                combinedCts.Dispose();
            });

            _cache.TryAdd(Context.ConnectionId, activeConnection);

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_cache.TryRemove(Context.ConnectionId, out var activeConnection))
                activeConnection.ReleaseResources();

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
