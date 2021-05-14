using Microsoft.Extensions.Logging;
using TypingRealm.Messaging.Client.Handling;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Messaging.Client
{
    public sealed class MessageProcessorFactory : IMessageProcessorFactory
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IClientConnectionFactoryFactory _clientConnectionFactoryFactory;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IProfileTokenProvider _profileTokenProvider;
        private readonly IClientToServerMessageMetadataFactory _metadataFactory;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly IAuthenticationService _authenticationService;

        public MessageProcessorFactory(
            ILogger<MessageProcessor> logger,
            IClientConnectionFactoryFactory clientConnectionFactoryFactory,
            IMessageDispatcher messageDispatcher,
            IProfileTokenProvider profileTokenProvider,
            IClientToServerMessageMetadataFactory metadataFactory,
            IMessageTypeCache messageTypeCache,
            IAuthenticationService authenticationService)
        {
            _logger = logger;
            _clientConnectionFactoryFactory = clientConnectionFactoryFactory;
            _messageDispatcher = messageDispatcher;
            _profileTokenProvider = profileTokenProvider;
            _metadataFactory = metadataFactory;
            _messageTypeCache = messageTypeCache;
            _authenticationService = authenticationService;
        }

        public IMessageProcessor CreateMessageProcessorFor(string connectionString)
        {
            var clientConnectionFactory = _clientConnectionFactoryFactory.CreateClientConnectionFactoryFor(connectionString);

            return new MessageProcessor(
                _logger,
                clientConnectionFactory,
                _messageDispatcher,
                _profileTokenProvider,
                _metadataFactory,
                _messageTypeCache,
                _authenticationService);
        }
    }
}
