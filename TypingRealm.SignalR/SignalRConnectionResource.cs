using System;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.SignalR
{
    public sealed class SignalRConnectionResource
    {
        private readonly Func<Task> _cancel;

        public SignalRConnectionResource(
            Notificator notificator,
            Func<Task> cancel)
        {
            Notificator = notificator;
            _cancel = cancel;
        }

        public Notificator Notificator { get; }

        public Task CancelAsync() => _cancel();
    }
}
