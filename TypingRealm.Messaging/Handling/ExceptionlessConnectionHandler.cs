using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Messaging.Handling
{
    public class ExceptionlessConnectionHandler : IConnectionHandler
    {
        private readonly IConnectionHandler _connectionHandler;
        private readonly ILogger<ExceptionlessConnectionHandler> _logger;

        public ExceptionlessConnectionHandler(
            IConnectionHandler connectionHandler,
            ILogger<ExceptionlessConnectionHandler> logger)
        {
            _connectionHandler = connectionHandler;
            _logger = logger;
        }

        public Task HandleAsync(IConnection connection, CancellationToken cancellationToken)
        {
            return _connectionHandler.HandleAsync(connection, cancellationToken)
                .HandleCancellationAsync(exception =>
                {
                    _logger.LogInformation(exception, $"Connection {connection} has been canceled.");
                })
                .HandleExceptionAsync<Exception>(exception =>
                {
                    _logger.LogError(exception, $"Connection {connection} died.");
                });
        }
    }
}
