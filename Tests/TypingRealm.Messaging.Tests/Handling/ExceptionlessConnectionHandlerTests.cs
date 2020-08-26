using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using TypingRealm.Messaging.Handling;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling
{
    public class ExceptionlessConnectionHandlerTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldHandleCancellationException(
            IConnection connection,
            OperationCanceledException exception,
            [Frozen]Mock<IConnectionHandler> connectionHandler,
            [Frozen]Mock<ILogger<ExceptionlessConnectionHandler>> logger,
            ExceptionlessConnectionHandler sut)
        {
            connectionHandler.Setup(x => x.HandleAsync(connection, Cts.Token))
                .ThrowsAsync(exception);

            await sut.HandleAsync(connection, Cts.Token);

            Assert.NotNull(logger.Invocations);
            Assert.Single(logger.Invocations);
            Assert.Equal(LogLevel.Information, logger.Invocations.First().Arguments[0]);
            Assert.Equal(exception, logger.Invocations.First().Arguments[3]);
            Assert.Contains("cancel", logger.Invocations.First().Arguments[2].ToString());
        }

        [Theory, AutoMoqData]
        public async Task ShouldHandleException(
            IConnection connection,
            Exception exception,
            [Frozen]Mock<IConnectionHandler> connectionHandler,
            [Frozen]Mock<ILogger<ExceptionlessConnectionHandler>> logger,
            ExceptionlessConnectionHandler sut)
        {
            connectionHandler.Setup(x => x.HandleAsync(connection, Cts.Token))
                .ThrowsAsync(exception);

            await sut.HandleAsync(connection, Cts.Token);

            Assert.NotNull(logger.Invocations);
            Assert.Single(logger.Invocations);
            Assert.Equal(LogLevel.Error, logger.Invocations.First().Arguments[0]);
            Assert.Equal(exception, logger.Invocations.First().Arguments[3]);
        }
    }
}
