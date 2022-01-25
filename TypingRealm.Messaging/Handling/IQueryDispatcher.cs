using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Handling;

public interface IQueryDispatcher
{
    ValueTask<object> DispatchAsync(
        ConnectedClient sender,
        object message,
        Type responseType,
        CancellationToken cancellationToken);
}
