namespace TypingRealm.Messaging;

/// <summary>
/// Used to create a unique message identifier before sending it to the second party.
/// For now it's only used by client side, but it can be a universal interface.
/// </summary>
public interface IMessageIdFactory
{
    string CreateMessageId();
}
