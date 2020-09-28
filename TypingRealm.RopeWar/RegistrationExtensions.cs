using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.RopeWar.Adapters;
using TypingRealm.RopeWar.Handlers;

namespace TypingRealm.RopeWar
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterRopeWar(this IServiceCollection services)
        {
            services.AddSingleton<IContestStore, InMemoryContestStore>();

            services.AddTransient<ICharacterStateService, CharacterStateServiceAdapter>();

            services.AddProfileApiClients();

            return services
                .AddTransient<IConnectHook, ConnectHook>()
                .UseUpdateFactory<ContestUpdateFactory>()
                .RegisterHandler<JoinContest, JoinContestHandler>()
                .RegisterHandler<StartContest, StartContestHandler>()
                .RegisterHandler<PullRope, PullRopeHandler>();
        }
    }
}
