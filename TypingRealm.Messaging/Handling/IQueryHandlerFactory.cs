using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Handling;

public interface IQueryHandlerFactory
{
    ValueTask<object> GetHandlerAndHandleAsync(
        ConnectedClient sender,
        object queryMessage,
        Type responseType,
        CancellationToken cancellationToken);
}
