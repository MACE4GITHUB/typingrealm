using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

// Passing cancellation token to Stream is not tested.
public class ProtobufIntegrationTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldWork(
        [Frozen] Stream stream,
        [Frozen] Mock<IProtobufFieldNumberCache> cache,
        TestMessage message,
        List<string> list)
    {
        var protobuf = new ProtobufStreamSerializer(Array.Empty<Type>(), null!);

        var sut = new ProtobufConnection(stream, cache.Object, protobuf);

        cache.Setup(x => x.GetFieldNumber(typeof(TestMessage)))
            .Returns(2);

        cache.Setup(x => x.GetFieldNumber(typeof(List<string>)))
            .Returns(3);
        cache.Setup(x => x.GetTypeByFieldNumber(3))
            .Returns(typeof(List<string>));

        // Should throw when message not registered.
        await AssertThrowsAsync<InvalidOperationException>(
            () => sut.SendAsync(message, Cts.Token));

        // List should be registered by default.
        await sut.SendAsync(list, Cts.Token);

        // Should send and receive.
        await sut.SendAsync(list, Cts.Token);
        Assert.NotEqual(0, stream.Length);

        stream.Seek(0, SeekOrigin.Begin);

        // Should read first message.
        var result = (List<string>)await sut.ReceiveAsync(Cts.Token);
        Assert.False(ReferenceEquals(list, result));
        Assert.Equal(list, result);

        // Should read second message.
        result = (List<string>)await sut.ReceiveAsync(Cts.Token);
        Assert.False(ReferenceEquals(list, result));
        Assert.Equal(list, result);

        // There should be no more messages.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ReceiveAsync(Cts.Token).AsTask());
    }
}
