using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.SignalR
{
    public interface ISignalRConnectionFactory
    {
        // TODO: Possibly move out client code to separate assembly, remove SignalR.Client nuget package from this assembly.
        IConnection CreateProtobufConnectionForClient(HubConnection hub);
        IConnection CreateProtobufConnectionForServer(IClientProxy clientProxy, Notificator notificator);
    }
}
