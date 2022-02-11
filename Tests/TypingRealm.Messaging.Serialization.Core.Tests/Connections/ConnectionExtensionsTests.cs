using TypingRealm.Messaging.Serialization.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Connections;

public class ConnectionExtensionsTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldWrapConnectionInMessageSerializerConnection(
        IConnection connection,
        IMessageSerializer messageSerializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory messageMetadataFactory)
    {
        var sut = connection.AddCoreMessageSerialization(
            messageSerializer, messageTypeCache, messageMetadataFactory);

        Assert.IsType<MessageSerializerConnection>(sut);
        Assert.Equal(connection, GetPrivateField(sut, "_connection"));
        Assert.Equal(messageSerializer, GetPrivateField(sut, "_serializer"));
        Assert.Equal(messageTypeCache, GetPrivateField(sut, "_messageTypeCache"));
        Assert.Equal(messageMetadataFactory, GetPrivateField(sut, "_metadataFactory"));
    }
}
