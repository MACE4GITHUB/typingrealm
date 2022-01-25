using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Handling;

// TODO: Unit test.
public sealed class QueryHandlerFactory : IQueryHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly QueryHandlerMethodCache _methodCache;

    public QueryHandlerFactory(
        IServiceProvider serviceProvider,
        QueryHandlerMethodCache methodCache)
    {
        _serviceProvider = serviceProvider;
        _methodCache = methodCache;
    }

    public async ValueTask<object> GetHandlerAndHandleAsync(
        ConnectedClient sender,
        object queryMessage,
        Type responseType,
        CancellationToken cancellationToken)
    {
        var methodInfo = _methodCache.GetHandleMethod(queryMessage.GetType(), responseType);

        var result = methodInfo.Invoke(this, new[]
        {
                sender,
                queryMessage,
                cancellationToken
            });

        if (result == null)
            throw new InvalidOperationException();

        var task = (Task)result;
        await task.ConfigureAwait(false);

        return ((dynamic)task).Result;
    }

    // Unfortunately we need to use Task here so we can cast it to non-generic Task.
    // Do not delete this method, it is called dynamically.
    private async Task<TResponse> HandleAsync<TQueryMessage, TResponse>(
        ConnectedClient sender,
        TQueryMessage queryMessage,
        CancellationToken cancellationToken)
    {
        var queryHandler = _serviceProvider.GetRequiredService<IQueryHandler<TQueryMessage, TResponse>>();

        return await queryHandler.HandleAsync(sender, queryMessage, cancellationToken)
            .ConfigureAwait(false);
    }
}
