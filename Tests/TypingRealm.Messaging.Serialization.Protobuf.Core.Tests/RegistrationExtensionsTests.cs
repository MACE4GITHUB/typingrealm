using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProtoBuf.Meta;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

public class RegistrationExtensionsTests : TestsBase
{
    [Fact]
    public void AddProtobuf_ShouldRegisterProtobufConnectionFactory()
    {
        var sut = new ServiceCollection();
        sut.AddSerializationCore();
        sut.AddProtobuf();

        var provider = sut.BuildServiceProvider();
        provider.AssertRegisteredTransient<IProtobufConnectionFactory, ProtobufConnectionFactory>();
    }

    [Fact]
    public void AddProtobuf_ShouldRegisterProtobufFieldNumberCacheSingleton()
    {
        var sut = new ServiceCollection();
        sut.AddTransient<IMessageTypeCache, MessageTypeCache>();

        sut.AddProtobuf();

        var provider = sut.BuildServiceProvider();
        provider.AssertRegisteredSingleton<IProtobufFieldNumberCache, ProtobufFieldNumberCache>();
    }

    [Fact]
    public void AddProtobuf_ShouldRegisterProtobufStreamSerializerAsTransientWith3Messages()
    {
        var sut = new ServiceCollection();
        sut.AddTransient<IMessageTypeCache, MessageTypeCache>();

        sut.AddProtobuf();

        var provider = sut.BuildServiceProvider();
        provider.AssertRegisteredTransient<IProtobufStreamSerializer, ProtobufStreamSerializer>();

        var serializer = provider.GetRequiredService<IProtobufStreamSerializer>();
        var typeMembers = ((RuntimeTypeModel)GetPrivateField(serializer, "_model")!).GetTypes().Cast<MetaType>()
            .Select(x => x.Type)
            .ToList();

        Assert.Equal(3, typeMembers.Count);
        Assert.Contains(typeof(MessageData), typeMembers);
        Assert.Contains(typeof(MessageMetadata), typeMembers);
        Assert.Contains(typeof(ClientToServerMessageMetadata), typeMembers);
    }

    [Fact]
    public void AddProtobuf_ShouldRegisterMessageSerializerAsProtobuf()
    {
        var sut = new ServiceCollection();
        sut.AddTransient<IMessageTypeCache, MessageTypeCache>();

        sut.AddProtobuf();

        var provider = sut.BuildServiceProvider();
        provider.AssertRegisteredTransient<IMessageSerializer, ProtobufMessageSerializer>();
    }

    [Fact]
    public void AddProtobuf_ShouldUseMessageTypeCacheForProtobufSerializer()
    {
        var messageTypes = new[]
        {
                typeof(HashSet<int>),
                typeof(MessageAttribute),
                typeof(List<string>)
            };

        var sut = new ServiceCollection();
        sut.AddTransient<IMessageTypeCache>(_ => new MessageTypeCache(messageTypes));

        sut.AddProtobuf();

        var provider = sut.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var registeredTypes = ((RuntimeTypeModel)GetPrivateField(serializer, "_model")!).GetTypes().Cast<MetaType>()
            .Select(t => t.Type)
            .ToList();

        Assert.Equal(3, registeredTypes.Count);
        foreach (var type in messageTypes)
        {
            Assert.Contains(type, registeredTypes);
        }
    }

    [Fact]
    public void AddProtobufMessageSerializer_ShouldUseMessageTypeCacheForProtobufSerializer()
    {
        var messageTypes = new[]
        {
                typeof(HashSet<int>),
                typeof(MessageAttribute),
                typeof(List<string>)
            };

        var sut = new ServiceCollection();
        sut.AddTransient<IMessageTypeCache>(_ => new MessageTypeCache(messageTypes));

        sut.AddProtobufMessageSerializer();

        var provider = sut.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var registeredTypes = ((RuntimeTypeModel)GetPrivateField(serializer, "_model")!).GetTypes().Cast<MetaType>()
            .Select(t => t.Type)
            .ToList();

        Assert.Equal(3, registeredTypes.Count);
        foreach (var type in messageTypes)
        {
            Assert.Contains(type, registeredTypes);
        }
    }

    private class AMessage
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
    public class BMessage { }
    // This test adds AMessage and BMessage to RuntimeTypeModel.
    [Theory, AutoMoqData]
    public void AddProtobuf_ShouldCreateProtobufStreamSerializerWithThreeRawTypes(Mock<IMessageTypeCache> cache)
    {
        var sut = new ServiceCollection();
        sut.AddSingleton(cache.Object);

        sut.AddProtobuf();
        var provider = sut.BuildServiceProvider();

        var types = new List<KeyValuePair<string, Type>>
            {
                new KeyValuePair<string, Type>("1", typeof(AMessage)),
                new KeyValuePair<string, Type>("1", typeof(BMessage))
            };

        cache.Setup(x => x.GetAllTypes())
            .Returns(types);

        var protobuf = provider.GetRequiredService<IProtobufStreamSerializer>();
        var registeredTypes = ((RuntimeTypeModel)GetPrivateField(protobuf, "_model")!).GetTypes().Cast<MetaType>()
            .Select(t => t.Type)
            .ToList();
        Assert.Equal(3, registeredTypes.Count);
        Assert.Contains(typeof(MessageData), registeredTypes);
        Assert.Contains(typeof(MessageMetadata), registeredTypes);
        Assert.Contains(typeof(ClientToServerMessageMetadata), registeredTypes);

        var typeMembers = ((RuntimeTypeModel)GetPrivateField(protobuf, "_model")!).GetTypes().Cast<MetaType>()
            .Single(x => x.Type == typeof(MessageData))
            .GetFields()
            .ToList();

        var member1 = typeMembers.Single(t => t.FieldNumber == 1);
        var member2 = typeMembers.Single(t => t.FieldNumber == 2);
        var member3 = typeMembers.Single(t => t.FieldNumber == 3);

        Assert.Equal(typeof(string), member1.MemberType);
        Assert.Equal(nameof(MessageData.Data), member1.Name);
        Assert.Equal(typeof(MessageMetadata), member2.MemberType); // TODO: Test that derived classes are properly serialized.
        Assert.Equal(nameof(MessageData.Metadata), member2.Name);
        Assert.Equal(typeof(string), member3.MemberType);
        Assert.Equal(nameof(MessageData.TypeId), member3.Name);
    }
}
