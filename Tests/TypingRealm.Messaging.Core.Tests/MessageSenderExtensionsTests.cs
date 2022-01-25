using System.Threading.Tasks;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageSenderExtensionsTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task SendAsync_ShouldWrapClientMessageToMessageWithMetadata(
        object message,
        ClientToServerMessageMetadata metadata,
        IMessageSender sut)
    {
        await sut.SendAsync(message, metadata, Cts.Token);

        Mock.Get(sut).Verify(x => x.SendAsync(It.Is<ClientToServerMessageWithMetadata>(
            y => y.Message == message && y.Metadata == metadata), Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task SendAsync_ShouldWrapServerMessageToMessageWithMetadata(
        object message,
        ServerToClientMessageMetadata metadata,
        IMessageSender sut)
    {
        await sut.SendAsync(message, metadata, Cts.Token);

        Mock.Get(sut).Verify(x => x.SendAsync(It.Is<ServerToClientMessageWithMetadata>(
            y => y.Message == message && y.Metadata == metadata), Cts.Token));
    }
}
