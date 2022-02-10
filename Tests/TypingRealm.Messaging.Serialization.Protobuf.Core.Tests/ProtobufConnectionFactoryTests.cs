using System.IO;
using AutoFixture.Xunit2;
using TypingRealm.Messaging.Serialization.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

public class ProtobufConnectionFactoryTests : TestsBase
{
    [Theory, AutoMoqData]
    public void CreateProtobufConnection_ShouldCreateWithProperDependencies(
        [Frozen] IProtobufFieldNumberCache cache,
        [Frozen] IProtobufStreamSerializer protobuf,
        [Frozen] IMessageSerializer serializer,
        [Frozen] IMessageTypeCache messageTypeCache,
        [Frozen] IMessageMetadataFactory metadataFactory,
        Stream stream,
        ProtobufConnectionFactory sut)
    {
        var connection = sut.CreateProtobufConnection(stream);

        Assert.IsType<MessageSerializerConnection>(connection);

        Assert.Equal(serializer, GetPrivateField(connection, "_serializer"));
        Assert.Equal(messageTypeCache, GetPrivateField(connection, "_messageTypeCache"));
        Assert.Equal(metadataFactory, GetPrivateField(connection, "_metadataFactory"));

        var rawConnection = GetPrivateField(connection, "_connection")!;
        Assert.NotNull(rawConnection);
        Assert.IsType<ProtobufConnection>(rawConnection);

        Assert.Equal(cache, GetPrivateField(rawConnection, "_fieldNumberCache"));
        Assert.Equal(stream, GetPrivateField(rawConnection, "_stream"));
        Assert.Equal(protobuf, GetPrivateField(rawConnection, "_protobuf"));
    }
}
