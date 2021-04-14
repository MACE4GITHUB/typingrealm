using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Client.Handling
{
    public sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IMessageHandler<TMessage>> GetHandlersFor<TMessage>()
        {
            return _serviceProvider.GetServices<IMessageHandler<TMessage>>();
        }
    }
}
