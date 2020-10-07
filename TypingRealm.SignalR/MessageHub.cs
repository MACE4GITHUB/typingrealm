using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.SignalR
{
    public sealed class MessageHub : Hub
    {
        private readonly ISignalRServer _server;

        public MessageHub(ISignalRServer server)
        {
            _server = server;
        }

        public void Send(JsonSerializedMessage message)
        {
            _server.NotifyReceived(Context.ConnectionId, message);
        }

        public override Task OnConnectedAsync()
        {
            _server.StartHandling(Context, Clients.Caller);

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _server.StopHandling(Context.ConnectionId).ConfigureAwait(false);

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
