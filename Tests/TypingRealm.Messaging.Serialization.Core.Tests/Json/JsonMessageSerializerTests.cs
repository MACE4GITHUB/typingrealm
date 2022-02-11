using System;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Serialization;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json;

public class JsonMessageSerializerTests : TestsBase
{
    [Theory, AutoMoqData]
    public void Deserialize_ShouldThrowWhenNotSuccessful(
        [Frozen]Mock<ISerializer> serializer,
        JsonMessageSerializer sut,
        string data, Type type)
    {
        serializer.Setup(x => x.Deserialize(data, type))
            .Returns(null);

        Assert.Throws<InvalidOperationException>(
            () => sut.Deserialize(data, type));
    }

    [Theory, AutoMoqData]
    public void Deserialize_ShouldDeserialize(
        [Frozen]Mock<ISerializer> serializer,
        JsonMessageSerializer sut,
        string data, Type type, object deserialized)
    {
        serializer.Setup(x => x.Deserialize(data, type))
            .Returns(deserialized);

        var result = sut.Deserialize(data, type);
        Assert.Equal(deserialized, result);
    }

    [Theory, AutoMoqData]
    public void Serialize_ShouldSerialize(
        [Frozen]Mock<ISerializer> serializer,
        JsonMessageSerializer sut,
        object data, string serialized)
    {
        serializer.Setup(x => x.Serialize(data))
            .Returns(serialized);

        var result = sut.Serialize(data);
        Assert.Equal(serialized, result);
    }
}
