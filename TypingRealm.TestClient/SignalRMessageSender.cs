using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging;

namespace TypingRealm.TestClient
{
    public sealed class SignalRMessageSender : IMessageSender
    {
        private readonly HubConnection _hub;

        public SignalRMessageSender(HubConnection hub)
        {
            _hub = hub;
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            await _hub.SendAsync("Send", message, cancellationToken).ConfigureAwait(false);
        }
    }
}
