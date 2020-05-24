using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization
{
    /// <summary>
    /// Registers <see cref="MessageTypeCache"/> as singleton.
    /// </summary>
    public sealed class MessageTypeCacheBuilder
    {
        private readonly HashSet<Type> _messageTypes = new HashSet<Type>();
        private readonly List<Action<MessageTypeCache>> _postBuildActions
            = new List<Action<MessageTypeCache>>();

        /// <summary>
        /// ServiceCollection instance where this MessageTypeCache is registered
        /// as singleton.
        /// </summary>
        public IServiceCollection Services { get; }

        public MessageTypeCacheBuilder(IServiceCollection services)
        {
            Services = services;

            services.AddSingleton<IMessageTypeCache>(provider => Build());
        }

        /// <summary>
        /// Adds message type to the <see cref="IMessageTypeCache"/>.
        /// </summary>
        public MessageTypeCacheBuilder AddMessageType(Type messageType)
        {
            _messageTypes.Add(messageType);
            return this;
        }

        /// <summary>
        /// Adds an action that will be executed when the instance of
        /// <see cref="IMessageTypeCache"/> will be first constructed.
        /// </summary>
        public void AddPostBuildAction(Action<IMessageTypeCache> action)
            => _postBuildActions.Add(action);

        /// <summary>
        /// Builds an instance of <see cref="MessageTypeCache"/>. This method
        /// should be called only once when the container first requests the
        /// singleton instance.
        /// </summary>
        private MessageTypeCache Build()
        {
            var cache = new MessageTypeCache(_messageTypes);

            foreach (var action in _postBuildActions)
            {
                action(cache);
            }

            return cache;
        }
    }
}
