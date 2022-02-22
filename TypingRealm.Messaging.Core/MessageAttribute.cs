using System;

namespace TypingRealm.Messaging;

/// <summary>
/// Indicates that type is serializable message that can be sent over
/// <see cref="IConnection"/> implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageAttribute : Attribute
{
    public MessageAttribute(bool sendUpdate = false)
    {
        SendUpdate = sendUpdate;
    }

    public bool SendUpdate { get; }
}
