using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json
{
    public class MyMessage
    {
        public int Age { get; set; }
        public string? LastName { get; set; }
        public List<int>? List { get; set; }
    }

    public class JsonConnectionTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldSerializeAndSend(
            [Frozen]Mock<IMessageTypeCache> cache,
            [Frozen]Mock<IConnection> connection,
            string typeId,
            MyMessage message,
            JsonConnection sut)
        {
            cache.Setup(x => x.GetTypeId(typeof(MyMessage)))
                .Returns(typeId);

            JsonSerializedMessage sent = null!;
            connection.Setup(x => x.SendAsync(It.IsAny<object>(), Cts.Token))
                .Callback<object, CancellationToken>((message, ct) => sent = (JsonSerializedMessage)message);

            await sut.SendAsync(message, Cts.Token);
            Assert.Equal(typeId, sent.TypeId);
            Assert.Equal(JsonSerializer.Serialize(message), sent.Json);
        }

        [Theory, AutoMoqData]
        public async Task ShouldReceiveAndDeserialize(
            [Frozen]Mock<IMessageTypeCache> cache,
            [Frozen]Mock<IConnection> connection,
            MyMessage message,
            JsonSerializedMessage jsonMessage,
            JsonConnection sut)
        {
            jsonMessage.Json = JsonSerializer.Serialize(message);
            connection.Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(jsonMessage);
            cache.Setup(x => x.GetTypeById(jsonMessage.TypeId))
                .Returns(typeof(MyMessage));

            var result = (MyMessage)await sut.ReceiveAsync(Cts.Token);
            Assert.Equal(message.Age, result.Age);
            Assert.Equal(message.LastName, result.LastName);
            Assert.Equal(message.List, result.List);
        }
    }
}
