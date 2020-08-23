using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Handling
{
    public class ScopedConnectionHandler : IConnectionHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedConnectionHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleAsync(IConnection connection, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            // TODO: Use interface IConnectionHandler and Decorate it, instead of concrete class.
            // TODO: Test this class.
            // This class is not tested, it is difficult due to concrete ConnectionHandler class.
            await scope.ServiceProvider.GetRequiredService<ConnectionHandler>()
                .HandleAsync(connection, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
