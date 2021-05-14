using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.SignalR.Client;

namespace TypingRealm.World
{
    public static class RegistrationExtensions
    {
        public static MessageTypeCacheBuilder AddWorld(this MessageTypeCacheBuilder messageTypes)
        {
            var services = messageTypes.Services;

            services.AddSingleton<ILocationStore, LocationStore>();
            services.AddProfileApiClients();

            services
                .AddTransient<IConnectHook, ConnectHook>()
                .UseUpdateFactory<LocationUpdateFactory>()
                .RegisterHandler<JoinRopeWarContest, JoinRopeWarContestHandler>()
                .RegisterHandler<LeaveJoinedRopeWarContest, LeaveJoinedRopeWarContestHandler>()
                .RegisterHandler<MoveToLocation, MoveToLocationHandler>()
                .RegisterHandler<ProposeRopeWarContest, ProposeRopeWarContestHandler>()
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
