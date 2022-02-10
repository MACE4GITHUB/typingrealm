using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using TypingRealm.Messaging.Serialization.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Connections;

public class MessageSerializerConnectionTests : TestsBase
{
    private readonly Mock<IConnection> _connection;
    private readonly MessageSerializerConnection _sut;

    public MessageSerializerConnectionTests()
    {
        _connection = Freeze<IConnection>();
        _sut = Fixture.Create<MessageSerializerConnection>();
    }

    [Fact]
    public async Task ShouldThrow_WhenReceivedMessageIsNotMessageData()
    {
        _connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(Fixture.Create<object>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.ReceiveAsync(Cts.Token));
    }
}
