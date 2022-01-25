using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization;

/// <summary>
/// Registers <see cref="MessageTypeCache"/> as singleton.
/// </summary>
public sealed class MessageTypeCacheBuilder
{
    private readonly HashSet<Type> _messageTypes = new HashSet<Type>();

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
    /// Builds an instance of <see cref="MessageTypeCache"/>. This method
    /// should be called only once when the container first requests the
    /// singleton instance.
    /// </summary>
    private MessageTypeCache Build()
    {
        return new MessageTypeCache(_messageTypes);
    }
}
