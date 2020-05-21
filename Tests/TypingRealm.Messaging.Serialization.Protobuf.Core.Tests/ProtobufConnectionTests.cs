using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using ProtoBuf;
using ProtoBuf.Meta;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class AnotherMessage : TestMessage
    {
        public int IntValue { get; set; }
    }

    // Protobuf tests require static data so there is only one test.
    public class ProtobufConnectionTests
    {
        [Theory, AutoMoqData]
        public async Task ShouldWork(
            [Frozen]Stream stream,
            [Frozen]Mock<IMessageTypeCache> cache,
            TestMessage message,
            ProtobufConnection sut)
        {
            // Should throw when message not registered.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.SendAsync(message, default).AsTask());

            // Should send and receive.
            cache.Setup(x => x.GetTypeId(typeof(TestMessage))).Returns("10");
            cache.Setup(x => x.GetTypeById("10")).Returns(typeof(TestMessage));
            RuntimeTypeModel.Default.Add(typeof(TestMessage), false)
                .Add(nameof(message.Value));

            await sut.SendAsync(message, default);
            Assert.NotEqual(0, stream.Length);

            stream.Seek(0, SeekOrigin.Begin);
            var result = (TestMessage)await sut.ReceiveAsync(default);

            Assert.Equal(message.Value, result.Value);

            // Should throw if type is wrong in cache.
            RuntimeTypeModel.Default.Add(typeof(AnotherMessage), false)
                .Add("IntValue");
            cache.Setup(x => x.GetTypeById("10"))
                .Returns(typeof(AnotherMessage));
            stream.Seek(0, SeekOrigin.Begin);
            await Assert.ThrowsAsync<ProtoException>(
                () => sut.ReceiveAsync(default).AsTask());
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrowIfTypeIdIsNotInt(
            [Frozen]Mock<IMessageTypeCache> cache,
            TestMessage message,
            ProtobufConnection sut)
        {
            cache.Setup(x => x.GetTypeId(typeof(TestMessage)))
                .Returns("string");

            await Assert.ThrowsAsync<FormatException>(
                () => sut.SendAsync(message, default).AsTask());
        }
    }
}
