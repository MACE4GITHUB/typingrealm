using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Connecting
{
    public interface IConnectHook
    {
        ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken);
    }
}
