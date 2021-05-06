using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public interface IClientConnectionFactory
    {
        ValueTask<ConnectionWithDisconnect> ConnectAsync(CancellationToken cancellationToken);
    }
}
