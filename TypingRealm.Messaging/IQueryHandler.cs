using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging
{
    public interface IQueryHandler<in TQueryMessage, TResponse>
    {
        ValueTask<TResponse> HandleAsync(
            ConnectedClient sender,
            TQueryMessage queryMessage,
            CancellationToken cancellationToken);
    }
}
