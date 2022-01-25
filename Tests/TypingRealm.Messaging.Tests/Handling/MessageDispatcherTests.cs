using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Handling;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling;

public class MessageDispatcherTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldDispatchToMultipleHandlers(
        ConnectedClient sender,
        [Frozen] Mock<IMessageHandlerFactory> handlerFactory,
        TestMessage message,
        MessageDispatcher sut)
    {
        var handlers = Fixture.CreateMany<Mock<IMessageHandler<TestMessage>>>();
        handlerFactory.Setup(x => x.GetHandlersFor<TestMessage>())
            .Returns(handlers.Select(h => h.Object));

        await sut.DispatchAsync(sender, message, Cts.Token);

        foreach (var handler in handlers)
        {
            handler.Verify(x => x.HandleAsync(sender, message, Cts.Token));
        }
    }
}
