using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.TypingDuels;

public static class RegistrationExtensions
{
    public static MessageTypeCacheBuilder AddTypingDuelsMessages(this MessageTypeCacheBuilder builder)
    {
        builder.AddMessageTypesFromAssembly(typeof(Typed).Assembly);
        return builder;
    }

    public static MessageTypeCacheBuilder AddTypingDuelsDomain(this MessageTypeCacheBuilder builder)
    {
        builder.AddTypingDuelsMessages();
        builder.Services.RegisterHandler<Typed, TypeMessageHandler>();
        builder.Services.AddSingleton<TypedDebouncer>();
        builder.Services.UseUpdateFactory<UpdateFactory>();
        builder.Services.AddTransient<IConnectionGroupProvider, ConnectionGroupProvider>();
        return builder;
    }
}
