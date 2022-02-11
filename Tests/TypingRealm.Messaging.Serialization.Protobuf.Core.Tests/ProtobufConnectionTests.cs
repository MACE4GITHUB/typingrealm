using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

public class ProtobufConnectionTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldReceive(
        [Frozen] Stream stream,
        [Frozen] Mock<IProtobufStreamSerializer> protobuf,
        [Frozen] Mock<IProtobufFieldNumberCache> cache,
        TestMessage message,
        ProtobufConnection sut)
    {
        cache.Setup(x => x.GetTypeByFieldNumber(5)).Returns(typeof(TestMessage));

        protobuf.Setup(x => x.Deserialize(stream, It.Is<Func<int, Type>>(
            func => func.Invoke(5) == typeof(TestMessage))))
            .Returns(message);

        var result = await sut.ReceiveAsync(Cts.Token);

        Assert.Equal(message, result);
    }

    [Theory, AutoMoqData]
    public async Task ShouldSend(
        [Frozen] Stream stream,
        [Frozen] Mock<IProtobufStreamSerializer> protobuf,
        [Frozen] Mock<IProtobufFieldNumberCache> cache,
        byte[] writtenBytes,
        TestMessage message,
        ProtobufConnection sut)
    {
        cache.Setup(x => x.GetFieldNumber(typeof(TestMessage)))
            .Returns(5);

        Stream writeStream = null!;
        protobuf.Setup(x => x.Serialize(It.IsAny<MemoryStream>(), message, 5))
            .Callback<Stream, object, int>((stream, message, fieldNumber) =>
            {
                writeStream = stream;
                writeStream.Write(writtenBytes, 0, writtenBytes.Length);
            });

        await sut.SendAsync(message, Cts.Token);

        protobuf.Verify(x => x.Serialize(It.IsAny<MemoryStream>(), message, 5));

        // Temporary stream should be disposed.
        Assert.Throws<ObjectDisposedException>(
            () => writeStream.Read(Array.Empty<byte>(), 0, 0));

        // Should write the value to stream.
        Assert.NotEmpty(writtenBytes);
        var resultBytes = new byte[writtenBytes.Length];
        stream.Seek(0, SeekOrigin.Begin);
        await stream.ReadAsync(resultBytes.AsMemory(0, writtenBytes.Length));
        Assert.Equal(writtenBytes, resultBytes);
    }

    public class TestStream : MemoryStream
    {
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);

            return await base.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);

            await base.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }
    }
    [Theory, AutoMoqData]
    public async Task ShouldWait_ToReceive(
        Mock<IProtobufStreamSerializer> protobuf,
        Mock<IProtobufFieldNumberCache> cache,
        TestMessage message)
    {
        using var stream = new TestStream();
        Fixture.Register<Stream>(() => stream);
        Fixture.Register<IProtobufStreamSerializer>(() => protobuf.Object);
        Fixture.Register<IProtobufFieldNumberCache>(() => cache.Object);
        var sut = Create<ProtobufConnection>();

        cache.Setup(x => x.GetTypeByFieldNumber(5)).Returns(typeof(TestMessage));

        protobuf.Setup(x => x.Deserialize(stream, It.Is<Func<int, Type>>(
            func => func.Invoke(5) == typeof(TestMessage))))
            .Returns(message);

        var result = await sut.ReceiveAsync(Cts.Token);

        Assert.Equal(message, result);
    }

    [Theory, AutoMoqData]
    public async Task ShouldWaitToSend(
        Mock<IProtobufStreamSerializer> protobuf,
        Mock<IProtobufFieldNumberCache> cache,
        byte[] writtenBytes,
        TestMessage message)
    {
        using var stream = new TestStream();
        Fixture.Register<Stream>(() => stream);
        Fixture.Register<IProtobufStreamSerializer>(() => protobuf.Object);
        Fixture.Register<IProtobufFieldNumberCache>(() => cache.Object);
        var sut = Create<ProtobufConnection>();

        cache.Setup(x => x.GetFieldNumber(typeof(TestMessage)))
            .Returns(5);

        Stream writeStream = null!;
        protobuf.Setup(x => x.Serialize(It.IsAny<MemoryStream>(), message, 5))
            .Callback<Stream, object, int>((stream, message, fieldNumber) =>
            {
                writeStream = stream;
                writeStream.Write(writtenBytes, 0, writtenBytes.Length);
            });

        await sut.SendAsync(message, Cts.Token);

        protobuf.Verify(x => x.Serialize(It.IsAny<MemoryStream>(), message, 5));

        // Temporary stream should be disposed.
        Assert.Throws<ObjectDisposedException>(
            () => writeStream.Read(Array.Empty<byte>(), 0, 0));

        // Should write the value to stream.
        Assert.NotEmpty(writtenBytes);
        var resultBytes = new byte[writtenBytes.Length];
        stream.Seek(0, SeekOrigin.Begin);
        await stream.ReadAsync(resultBytes.AsMemory(0, writtenBytes.Length));
        Assert.Equal(writtenBytes, resultBytes);
    }
}
