using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.SignalR.Connections
{
    public sealed class ClientToServerSignalRMessageSender : IMessageSender
    {
        private readonly HubConnection _hub;

        private ClientToServerSignalRMessageSender(HubConnection hub, Notificator notificator)
        {
            _hub = hub;
            _hub.On<ServerToClientMessageData>(SignalRConstants.Send, message => notificator.NotifyReceived(message));
        }

        public static IConnection Create(HubConnection hub)
        {
            var notificator = new Notificator();

            return new ClientToServerSignalRMessageSender(hub, notificator)
                .WithNotificator(notificator);
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            await _hub.SendAsync(SignalRConstants.Send, message, cancellationToken).ConfigureAwait(false);
        }
    }
}
