using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Connecting
{
    public sealed class EmptyConnectHook : IConnectHook
    {
        public ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
        {
            return default;
        }
    }
}
