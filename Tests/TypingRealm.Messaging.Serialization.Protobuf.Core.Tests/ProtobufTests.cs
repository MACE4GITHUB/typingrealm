using System;
using System.IO;
using System.Linq;
using ProtoBuf.Meta;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class ProtobufTests : TestsBase
    {
        private class AMessage { }
        private class BMessage { }
        [Fact]
        public void ShouldAddAllTypesToRuntimeTypeModel_WhenCreated()
        {
            var sut = new Protobuf(new[] { typeof(AMessage), typeof(BMessage) });
            var typeMembers = ((RuntimeTypeModel)GetPrivateField(sut, "_model")!).GetTypes().Cast<MetaType>()
                .Select(x => x.Type);

            Assert.Contains(typeof(AMessage), typeMembers);
            Assert.Contains(typeof(BMessage), typeMembers);
        }

        private class CMessage { public string? Value { get; set; } }
        [Fact]
        public void ShouldSerializeAndDeserialize()
        {
            var message = Create<CMessage>();
            var protobuf = new Protobuf(new[] { typeof(CMessage) });

            using var stream = new MemoryStream();
            protobuf.Serialize(stream, message, 3);

            stream.Seek(0, SeekOrigin.Begin);
            var result = (CMessage)protobuf.Deserialize(stream, fieldNumber =>
            {
                if (fieldNumber == 3)
                    return typeof(CMessage);

                throw new InvalidOperationException();
            });

            Assert.Equal(message.Value, result.Value);
        }
    }
}
