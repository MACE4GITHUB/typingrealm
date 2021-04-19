using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Handling
{
    // TODO: Unit test.
    public sealed class QueryDispatcher : IQueryDispatcher
    {
        private readonly IQueryHandlerFactory _queryHandlerFactory;

        public QueryDispatcher(IQueryHandlerFactory queryHandlerFactory)
        {
            _queryHandlerFactory = queryHandlerFactory;
        }

        public ValueTask<object> DispatchAsync(ConnectedClient sender, object message, Type responseType, CancellationToken cancellationToken)
        {
            return _queryHandlerFactory.GetHandlerAndHandleAsync(sender, message, responseType, cancellationToken);
        }
    }
}
