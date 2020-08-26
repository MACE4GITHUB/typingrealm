using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Handling
{
    public class ScopedConnectionHandler : IScopedConnectionHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedConnectionHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleAsync(IConnection connection, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IConnectionHandler>()
                .HandleAsync(connection, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
