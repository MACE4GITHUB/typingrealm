using System;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.SignalR
{
    public sealed class SignalRConnectionResource
    {
        public SignalRConnectionResource(
            Notificator notificator,
            Func<Task> releaseResources)
        {
            Notificator = notificator;
            ReleaseResources = releaseResources;
        }

        public Notificator Notificator { get; }
        public Func<Task> ReleaseResources { get; }
    }
}
