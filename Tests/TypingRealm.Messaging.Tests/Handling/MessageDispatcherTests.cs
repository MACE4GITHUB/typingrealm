using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Handling;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling
{
    public class AnotherTestMessage : TestMessage { }

    public class MessageDispatcherTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldDispatchToMultipleHandlers(
            ConnectedClient sender,
            [Frozen]Mock<IMessageHandlerFactory> handlerFactory,
            CancellationToken ct,
            MessageDispatcher sut)
        {
            var handlers = Fixture.CreateMany<Mock<IMessageHandler<AnotherTestMessage>>>();
            handlerFactory.Setup(x => x.GetHandlersFor<AnotherTestMessage>())
                .Returns(handlers.Select(h => h.Object));
            TestMessage message = new AnotherTestMessage();

            await sut.DispatchAsync(sender, message, ct);

            foreach (var handler in handlers)
            {
                handler.Verify(x => x.HandleAsync(sender, (AnotherTestMessage)message, ct));
            }
        }
    }
}
