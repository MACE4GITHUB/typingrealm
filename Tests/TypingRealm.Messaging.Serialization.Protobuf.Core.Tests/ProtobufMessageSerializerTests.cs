using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

public class ProtobufMessageSerializerTests
{
#pragma warning disable CS8618
    public class TestMessage
    {
        public string Value { get; set; }
        public int Age { get; set; }
        public IEnumerable<string> Values { get; set; }

        // Uncomment this after it's implemented:
        // Implement nested types serialization even when they are not marked by Message attribute.
        //public SecondMessage Message { get; set; }
        //public IEnumerable<SecondMessage> SecondValues { get; set; }
    }

    /*public class SecondMessage
    {
        public int Age { get; set; }
        public List<string> Values { get; set; }
    }*/
#pragma warning restore CS8618

    [Fact]
    public void ShouldSerializeAndDeserialize()
    {
        var sut = new ProtobufMessageSerializer(new[] { typeof(TestMessage) });

        var serialized = sut.Serialize(new TestMessage
        {
            Value = "value",
            Age = 50,
            Values = new List<string> { "abc" }
        });

        var deserialized = sut.Deserialize(serialized, typeof(TestMessage)) as TestMessage;

        Assert.Equal("value", deserialized!.Value);
        Assert.Equal(50, deserialized!.Age);
        Assert.Single(deserialized.Values);
        Assert.Equal("abc", deserialized.Values.First());
    }
}

public class ProtobufStreamSerializerTests : TestsBase
{
    // It should work with only MessageData message, so no need to test other messages.

    [Fact]
    public void ShouldNotDeserialize_MessageWithWrongType()
    {
        var sut = GetServiceCollection()
            .AddProtobuf()
            .BuildServiceProvider()
            .GetRequiredService<IProtobufStreamSerializer>();

        using var stream = new MemoryStream();
        sut.Serialize(stream, new MessageData
        {
            TypeId = "typeId",
            Metadata = null,
            Data = "data"
        }, 5);

        stream.Seek(0, SeekOrigin.Begin);
        Assert.Throws<ProtoException>(() => sut.Deserialize(stream, number =>
        {
            if (number == 5)
                return typeof(MessageMetadata);

            return typeof(string);
        }));
    }

    [Fact]
    public void ShouldSerializeAndDeserialize_MessageWithoutMetadata()
    {
        var sut = GetServiceCollection()
            .AddProtobuf()
            .BuildServiceProvider()
            .GetRequiredService<IProtobufStreamSerializer>();

        using var stream = new MemoryStream();
        sut.Serialize(stream, new MessageData
        {
            TypeId = "typeId",
            Metadata = null,
            Data = "data"
        }, 5);

        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = sut.Deserialize(stream, number =>
        {
            if (number == 5)
                return typeof(MessageData);

            return typeof(MessageMetadata);
        }) as MessageData;

        Assert.Null(deserialized!.Metadata);
        Assert.Equal("data", deserialized.Data);
        Assert.Equal("typeId", deserialized.TypeId);
    }

    [Fact]
    public void ShouldSerializeAndDeserialize_MessageWithMetadata()
    {
        var sut = GetServiceCollection()
            .AddProtobuf()
            .BuildServiceProvider()
            .GetRequiredService<IProtobufStreamSerializer>();

        using var stream = new MemoryStream();
        sut.Serialize(stream, new MessageData
        {
            TypeId = "typeId",
            Metadata = new MessageMetadata
            {
                MessageId = "id",
                ResponseMessageTypeId = "responseId",
                AcknowledgementType = AcknowledgementType.Received
            },
            Data = "data"
        }, 5);

        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = sut.Deserialize(stream, number =>
        {
            if (number == 5)
                return typeof(MessageData);

            return typeof(MessageMetadata);
        }) as MessageData;

        Assert.IsType<MessageMetadata>(deserialized!.Metadata);
        Assert.Equal("id", deserialized!.Metadata!.MessageId);
        Assert.Equal("responseId", deserialized!.Metadata!.ResponseMessageTypeId);
        Assert.Equal(AcknowledgementType.Received, deserialized!.Metadata!.AcknowledgementType);
        Assert.Equal("data", deserialized.Data);
        Assert.Equal("typeId", deserialized.TypeId);
    }

    [Fact]
    public void ShouldSerializeAndDeserialize_MessageWithClientToServerMetadata()
    {
        var sut = GetServiceCollection()
            .AddProtobuf()
            .BuildServiceProvider()
            .GetRequiredService<IProtobufStreamSerializer>();

        using var stream = new MemoryStream();
        sut.Serialize(stream, new MessageData
        {
            TypeId = "typeId",
            Metadata = new ClientToServerMessageMetadata
            {
                MessageId = "id",
                ResponseMessageTypeId = "responseId",
                AcknowledgementType = AcknowledgementType.Received,
                AffectedGroups = new[] { "abc" }
            },
            Data = "data"
        }, 5);

        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = sut.Deserialize(stream, number =>
        {
            if (number == 5)
                return typeof(MessageData);

            return typeof(MessageMetadata);
        }) as MessageData;

        Assert.IsType<ClientToServerMessageMetadata>(deserialized!.Metadata);
        Assert.Equal("id", deserialized.Metadata!.MessageId);
        Assert.Equal("responseId", deserialized.Metadata!.ResponseMessageTypeId);
        Assert.Equal(AcknowledgementType.Received, deserialized!.Metadata!.AcknowledgementType);
        Assert.Equal("data", deserialized.Data);
        Assert.Equal("typeId", deserialized.TypeId);
        Assert.Equal(new[] { "abc" }, ((ClientToServerMessageMetadata)deserialized.Metadata).AffectedGroups);
    }

    [Fact]
    public void ShouldThrow_WhenInstanceForSerializationIsNull()
    {
        var sut = GetServiceCollection()
            .AddProtobuf()
            .BuildServiceProvider()
            .GetRequiredService<IProtobufStreamSerializer>();

        Assert.Throws<ArgumentNullException>(() => sut.Serialize(new MemoryStream(), null!, 10));
    }
}
