using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json;

public class MyMessage
{
    public int Age { get; set; }
    public string? LastName { get; set; }
    public List<int>? List { get; set; }
}

public enum TestEnum
{
    One = 1,
    Two = 2
}

public class TestEnumMessage
{
    public TestEnum TestEnum { get; set; }
}

public class JsonConnectionTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldSerializeAndSend_InCamelCase(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        string typeId,
        MyMessage message,
        JsonConnectionDeprecated sut)
    {
        cache.Setup(x => x.GetTypeId(typeof(MyMessage)))
            .Returns(typeId);

        JsonSerializedMessage sent = null!;
        connection.Setup(x => x.SendAsync(It.IsAny<object>(), Cts.Token))
            .Callback<object, CancellationToken>((message, ct) => sent = (JsonSerializedMessage)message);

        await sut.SendAsync(message, Cts.Token);
        Assert.Equal(typeId, sent.TypeId);
        Assert.Equal(
            JsonSerializer.Serialize(
                message,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
            sent.Json);
    }

    [Theory, AutoMoqData]
    public async Task ShouldReceiveAndDeserialize_InCamelCase(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        MyMessage message,
        JsonSerializedMessage jsonMessage,
        JsonConnectionDeprecated sut)
    {
        jsonMessage.Json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(jsonMessage);
        cache.Setup(x => x.GetTypeById(jsonMessage.TypeId))
            .Returns(typeof(MyMessage));

        var result = (MyMessage)await sut.ReceiveAsync(Cts.Token);
        Assert.Equal(message.Age, result.Age);
        Assert.Equal(message.LastName, result.LastName);
        Assert.Equal(message.List, result.List);
    }

    [Theory, AutoMoqData]
    public async Task ShouldDeserializeEnum_FromNumber(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        JsonSerializedMessage jsonMessage,
        JsonConnectionDeprecated sut)
    {
        jsonMessage.Json = "{\"testEnum\":2}";

        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(jsonMessage);
        cache.Setup(x => x.GetTypeById(jsonMessage.TypeId))
            .Returns(typeof(TestEnumMessage));

        var result = (TestEnumMessage)await sut.ReceiveAsync(Cts.Token);
        Assert.Equal(TestEnum.Two, result.TestEnum);
    }

    [Theory, AutoMoqData]
    public async Task ShouldDeserializeEnum_FromString(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        JsonSerializedMessage jsonMessage,
        JsonConnectionDeprecated sut)
    {
        jsonMessage.Json = "{\"testEnum\":\"Two\"}";

        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(jsonMessage);
        cache.Setup(x => x.GetTypeById(jsonMessage.TypeId))
            .Returns(typeof(TestEnumMessage));

        var result = (TestEnumMessage)await sut.ReceiveAsync(Cts.Token);
        Assert.Equal(TestEnum.Two, result.TestEnum);
    }

    [Theory, AutoMoqData]
    public async Task ShouldSerializeEnum_ToString(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        string typeId,
        JsonConnectionDeprecated sut)
    {
        cache.Setup(x => x.GetTypeId(typeof(TestEnumMessage)))
            .Returns(typeId);

        JsonSerializedMessage sent = null!;
        connection.Setup(x => x.SendAsync(It.IsAny<object>(), Cts.Token))
            .Callback<object, CancellationToken>((message, ct) => sent = (JsonSerializedMessage)message);

        await sut.SendAsync(new TestEnumMessage { TestEnum = TestEnum.Two }, Cts.Token);
        Assert.Equal(typeId, sent.TypeId);
        Assert.Equal("{\"testEnum\":\"Two\"}", sent.Json);
    }

    [Theory, AutoMoqData]
    public async Task ShouldThrow_WhenDeserializationReturnsNull(
        [Frozen] Mock<IMessageTypeCache> cache,
        [Frozen] Mock<IConnection> connection,
        JsonSerializedMessage jsonMessage,
        JsonConnectionDeprecated sut)
    {
        jsonMessage.Json = "null";

        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(jsonMessage);
        cache.Setup(x => x.GetTypeById(jsonMessage.TypeId))
            .Returns(typeof(MyMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sut.ReceiveAsync(Cts.Token));
    }
}
