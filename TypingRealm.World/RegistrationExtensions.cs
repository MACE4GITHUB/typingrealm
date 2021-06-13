using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.SignalR.Client;
using TypingRealm.World.Activities.RopeWar;
using TypingRealm.World.Layers;
using TypingRealm.World.Movement;

namespace TypingRealm.World
{
    public static class RegistrationExtensions
    {
        public static MessageTypeCacheBuilder AddWorld(this MessageTypeCacheBuilder messageTypes)
        {
            var services = messageTypes.Services;

            // Should be transient or scoped since it needs IClientsService.
            services.AddTransient<ILocationRepository, LocationRepository>();
            services.AddSingleton<LocationStore>();

            services.AddTransient<IActivityStore>(x => (IActivityStore)x.GetRequiredService<ILocationRepository>());
            services.AddTransient<ICharacterActivityStore, CharacterActivityStore>();
            services.AddProfileApiClients();

            services
                .AddTransient<IConnectHook, ConnectHook>()
                .UseUpdateFactory<WorldUpdateFactory>()
                .RegisterHandler<MoveToLocation, MoveToLocationHandler>()
                .RegisterHandler<JoinRopeWarContest, JoinRopeWarContestHandler>()
                .RegisterHandler<LeaveJoinedRopeWarContest, LeaveJoinedRopeWarContestHandler>()
                .RegisterHandler<ProposeRopeWarContest, ProposeRopeWarContestHandler>()
                .RegisterHandler<SwitchSides, SwitchSidesHandler>()
                .RegisterHandler<VoteToStartRopeWar, VoteToStartRopeWarHandler>();

            services.RegisterClientMessagingForServer<SignalRClientConnectionFactoryFactory>();

            messageTypes.AddWorldMessages();
            return messageTypes;
        }

        public static MessageTypeCacheBuilder AddWorldMessages(this MessageTypeCacheBuilder builder)
        {
            return builder.AddMessageTypesFromAssembly(typeof(JoinRopeWarContest).Assembly);
        }
    }
}
