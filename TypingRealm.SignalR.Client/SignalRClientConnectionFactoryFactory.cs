using TypingRealm.Messaging.Client;

namespace TypingRealm.SignalR.Client
{
    public sealed class SignalRClientConnectionFactoryFactory : IClientConnectionFactoryFactory
    {
        private readonly ISignalRConnectionFactory _factory;
        private readonly IProfileTokenProvider _profileTokenProvider;

        public SignalRClientConnectionFactoryFactory(
            ISignalRConnectionFactory factory,
            IProfileTokenProvider profileTokenProvider)
        {
            _factory = factory;
            _profileTokenProvider = profileTokenProvider;
        }

        public IClientConnectionFactory CreateClientConnectionFactoryFor(string connectionString)
        {
            return new SignalRClientConnectionFactory(
                _factory, _profileTokenProvider, connectionString);
        }
    }
}
