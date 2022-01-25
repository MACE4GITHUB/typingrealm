using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypingRealm.Messaging;

namespace TypingRealm.SignalR.Connections;

public sealed class ServerToClientSignalRMessageSender : IMessageSender
{
    private readonly IClientProxy _clientProxy;

    public ServerToClientSignalRMessageSender(IClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        await _clientProxy.SendAsync(SignalRConstants.Send, message, cancellationToken).ConfigureAwait(false);
    }
}
