using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json
{
    public class RegistrationExtensionsTests : TestsBase
    {
        [Fact]
        public void ShouldRegisterJsonConnectionFactory()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore().Services
                .AddJson()
                .BuildServiceProvider();

            provider.AssertRegisteredTransient<IJsonConnectionFactory, JsonConnectionFactory>();
        }
    }
}
