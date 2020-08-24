using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypingRealm.Messaging;

namespace TypingRealm.SignalR
{
    public sealed class SignalRMessageSender : IMessageSender
    {
        private readonly IClientProxy _clientProxy;

        public SignalRMessageSender(IClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            await _clientProxy.SendAsync("Send", message, cancellationToken).ConfigureAwait(false);
        }
    }
}
