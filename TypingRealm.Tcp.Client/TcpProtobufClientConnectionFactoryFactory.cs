using System;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.Tcp.Client
{
    public sealed class TcpProtobufClientConnectionFactoryFactory : IClientConnectionFactoryFactory
    {
        private readonly IProtobufConnectionFactory _factory;

        public TcpProtobufClientConnectionFactoryFactory(
            IProtobufConnectionFactory factory)
        {
            _factory = factory;
        }

        public IClientConnectionFactory CreateClientConnectionFactoryFor(string connectionString)
        {
            var host = connectionString.Split(",")[0];
            var port = Convert.ToInt32(connectionString.Split(",")[1]);

            return new TcpProtobufClientConnectionFactory(
                _factory, host, port);
        }
    }
}
