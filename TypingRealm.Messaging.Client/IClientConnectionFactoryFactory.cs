namespace TypingRealm.Messaging.Client;

public interface IClientConnectionFactoryFactory
{
    IClientConnectionFactory CreateClientConnectionFactoryFor(string connectionString);
}
