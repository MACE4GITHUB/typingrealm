using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class AMessage
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class BMessage
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class RegistrationExtensionsTests
    {
        [Theory, AutoMoqData]
        public void AddProtobuf_ShouldRegisterProtobufConnectionFactory()
        {
            var services = new ServiceCollection();
            var sut = new MessageTypeCacheBuilder(services);

            sut.AddProtobuf();

            var provider = services.BuildServiceProvider();
            AssertRegisteredTransient<IProtobufConnectionFactory, ProtobufConnectionFactory>(provider);
        }

        [Theory, AutoMoqData]
        public void AddProtobuf_ShouldAddAllRegisteredTypesToProtobufRuntimeTypeModel_WithAllProperties_OrderedByName()
        {
            var services = new ServiceCollection();
            var sut = new MessageTypeCacheBuilder(services);

            sut.AddProtobuf();
            sut.AddMessageType(typeof(AMessage));
            sut.AddMessageType(typeof(BMessage));

            var provider = services.BuildServiceProvider();
            var types = RuntimeTypeModel.Default.GetTypes().Cast<MetaType>().ToList();
            Assert.Empty(types);

            _ = provider.GetRequiredService<IMessageTypeCache>();
            types = RuntimeTypeModel.Default.GetTypes().Cast<MetaType>().ToList();
            Assert.Equal(2, types.Count);

            var type1 = types.Single(t => t.Type == typeof(AMessage));
            var type2 = types.Single(t => t.Type == typeof(BMessage));

            var type1Members = type1.GetFields().ToList();

            var member1 = type1Members.Single(t => t.FieldNumber == 1);
            var member2 = type1Members.Single(t => t.FieldNumber == 2);

            Assert.Equal(typeof(int), member1.MemberType);
            Assert.Equal(nameof(AMessage.Age), member1.Name);
            Assert.Equal(typeof(string), member2.MemberType);
            Assert.Equal(nameof(AMessage.Name), member2.Name);
        }

        private void AssertRegisteredTransient<TInterface, TImplementation>(IServiceProvider provider)
        {
            var implementation = provider.GetRequiredService<TInterface>();
            var implementation2 = provider.GetRequiredService<TInterface>();

            using var scope = provider.CreateScope();
            var implementation3 = scope.ServiceProvider.GetRequiredService<TInterface>();

            Assert.NotNull(implementation);
            Assert.IsType<TImplementation>(implementation);
            Assert.IsType<TImplementation>(implementation2);
            Assert.IsType<TImplementation>(implementation3);
            Assert.NotEqual(implementation, implementation2);
            Assert.NotEqual(implementation, implementation3);
        }
    }
}
