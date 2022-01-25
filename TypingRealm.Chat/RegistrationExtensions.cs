using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Chat;

public static class RegistrationExtensions
{
    public static MessageTypeCacheBuilder AddChat(this MessageTypeCacheBuilder messageTypes)
    {
        var services = messageTypes.Services;

        services
            .AddSingleton<MessageLog>()
            .UseUpdateFactory<StateFactory>()
            .RegisterHandler<Say, SayHandler>();

        messageTypes.AddChatMessages();
        return messageTypes;
    }

    public static MessageTypeCacheBuilder AddChatMessages(this MessageTypeCacheBuilder builder)
    {
        return builder.AddMessageTypesFromAssembly(typeof(Say).Assembly);
    }
}
