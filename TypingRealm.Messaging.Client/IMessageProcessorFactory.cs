namespace TypingRealm.Messaging.Client;

public interface IMessageProcessorFactory
{
    IMessageProcessor CreateMessageProcessorFor(string connectionString);
}
