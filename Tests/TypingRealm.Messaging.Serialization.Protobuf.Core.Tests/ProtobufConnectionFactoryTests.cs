using System.IO;
using AutoFixture.Xunit2;
using TypingRealm.Messaging.Serialization.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class ProtobufConnectionFactoryTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ForClient_ShouldCreateWithProperDependencies(
            [Frozen]IProtobufFieldNumberCache cache,
            [Frozen]IProtobufStreamSerializer protobuf,
            [Frozen]IMessageSerializer serializer,
            [Frozen]IMessageTypeCache messageTypeCache,
            [Frozen]IClientToServerMessageMetadataFactory metadataFactory,
            Stream stream,
            ProtobufConnectionFactory sut)
        {
            var connection = sut.CreateProtobufConnectionForClient(stream);

            Assert.IsType<ClientToServerSendingMessageSerializerConnection>(connection);

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

        [Theory, AutoMoqData]
        public void ForServer_ShouldCreateWithProperDependencies(
            [Frozen]IProtobufFieldNumberCache cache,
            [Frozen]IProtobufStreamSerializer protobuf,
            [Frozen]IMessageSerializer serializer,
            [Frozen]IMessageTypeCache messageTypeCache,
            Stream stream,
            ProtobufConnectionFactory sut)
        {
            var connection = sut.CreateProtobufConnectionForServer(stream);

            Assert.IsType<ServerToClientSendingMessageSerializerConnection>(connection);

            Assert.Equal(serializer, GetPrivateField(connection, "_serializer"));
            Assert.Equal(messageTypeCache, GetPrivateField(connection, "_messageTypeCache"));

            var rawConnection = GetPrivateField(connection, "_connection")!;
            Assert.NotNull(rawConnection);
            Assert.IsType<ProtobufConnection>(rawConnection);

            Assert.Equal(cache, GetPrivateField(rawConnection, "_fieldNumberCache"));
            Assert.Equal(stream, GetPrivateField(rawConnection, "_stream"));
            Assert.Equal(protobuf, GetPrivateField(rawConnection, "_protobuf"));
        }
    }
}
