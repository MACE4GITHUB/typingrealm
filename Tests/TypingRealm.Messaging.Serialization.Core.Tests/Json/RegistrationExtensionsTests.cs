using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json
{
    public class RegistrationExtensionsTests : TestsBase
    {
        [Fact]
        public void AddJson_ShouldRegisterJsonConnectionFactory()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddJson()
                .Services.BuildServiceProvider();

            provider.AssertRegisteredTransient<IJsonConnectionFactory, JsonConnectionFactory>();
        }

        [Fact]
        public void AddJson_ShouldAddJsonSerializedMessageToCache()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddJson()
                .Services.BuildServiceProvider();

            var cache = provider.GetRequiredService<IMessageTypeCache>();

            Assert.NotNull(cache.GetTypeId(typeof(JsonSerializedMessage)));
        }
    }
}
