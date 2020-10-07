using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TypingRealm.SignalR
{
    public interface ISignalRServer
    {
        void NotifyReceived(string connectionId, object message);
        void StartHandling(HubCallerContext context, IClientProxy caller);
        Task StopHandling(string connectionId);
    }
}
