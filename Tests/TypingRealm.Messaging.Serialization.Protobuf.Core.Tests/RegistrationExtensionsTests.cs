using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProtoBuf.Meta;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class RegistrationExtensionsTests : TestsBase
    {
        [Fact]
        public void AddProtobuf_ShouldRegisterProtobufConnectionFactory()
        {
            var sut = new ServiceCollection();
            sut.AddTransient<IMessageTypeCache, MessageTypeCache>();

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
        public void AddProtobuf_ShouldRegisterProtobufSingleton()
        {
            var sut = new ServiceCollection();
            sut.AddTransient<IMessageTypeCache, MessageTypeCache>();

            sut.AddProtobuf();

            var provider = sut.BuildServiceProvider();
            provider.AssertRegisteredSingleton<IProtobuf, Protobuf>();
        }

        private class AMessage
        {
            public string? Name { get; set; }
            public int Age { get; set; }
        }
        public class BMessage { }
        // This test adds AMessage and BMessage to RuntimeTypeModel.
        [Theory, AutoMoqData]
        public void AddProtobuf_ShouldCreateProtobufWithTypesFromMessageTypeCache(Mock<IMessageTypeCache> cache)
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

            var registeredTypes = RuntimeTypeModel.Default.GetTypes().Cast<MetaType>()
                .Select(t => t.Type)
                .ToList();
            Assert.DoesNotContain(typeof(AMessage), registeredTypes);
            Assert.DoesNotContain(typeof(BMessage), registeredTypes);

            _ = provider.GetRequiredService<IProtobuf>();
            registeredTypes = RuntimeTypeModel.Default.GetTypes().Cast<MetaType>()
                .Select(t => t.Type)
                .ToList();
            Assert.Contains(typeof(AMessage), registeredTypes);
            Assert.Contains(typeof(BMessage), registeredTypes);

            var typeMembers = RuntimeTypeModel.Default.GetTypes().Cast<MetaType>()
                .Single(x => x.Type == typeof(AMessage))
                .GetFields()
                .ToList();

            var member1 = typeMembers.Single(t => t.FieldNumber == 1);
            var member2 = typeMembers.Single(t => t.FieldNumber == 2);

            Assert.Equal(typeof(int), member1.MemberType);
            Assert.Equal(nameof(AMessage.Age), member1.Name);
            Assert.Equal(typeof(string), member2.MemberType);
            Assert.Equal(nameof(AMessage.Name), member2.Name);
        }
    }
}
