using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ConnectedClientTests
    {
        [Theory, AutoMoqData]
        public void ShouldBeCreatedUsingConstructor(string clientId, IConnection connection)
        {
            var client = new ConnectedClient(clientId, connection);

            Assert.Equal(clientId, client.ClientId);
            Assert.Equal(connection, client.Connection);
        }
    }
}
