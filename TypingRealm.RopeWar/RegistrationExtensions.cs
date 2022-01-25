using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.RopeWar.Adapters;
using TypingRealm.RopeWar.Handlers;

namespace TypingRealm.RopeWar;

public static class RegistrationExtensions
{
    public static MessageTypeCacheBuilder AddRopeWar(this MessageTypeCacheBuilder messageTypes)
    {
        var services = messageTypes.Services;

        services.AddSingleton<IContestStore, InMemoryContestStore>();
        services.AddTransient<ICharacterStateService, CharacterStateServiceAdapter>();
        services.AddProfileApiClients();

        services
            .AddTransient<IConnectHook, ConnectHook>()
            .UseUpdateFactory<ContestUpdateFactory>()
            .RegisterHandler<JoinContest, JoinContestHandler>()
            .RegisterHandler<StartContest, StartContestHandler>()
            .RegisterHandler<PullRope, PullRopeHandler>();

        messageTypes.AddRopeWarMessages();
        return messageTypes;
    }

    public static MessageTypeCacheBuilder AddRopeWarMessages(this MessageTypeCacheBuilder builder)
    {
        return builder.AddMessageTypesFromAssembly(typeof(JoinContest).Assembly);
    }
}
