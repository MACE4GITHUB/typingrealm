using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.RopeWar.Handlers;

namespace TypingRealm.RopeWar
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterRopeWar(this IServiceCollection services)
        {
            services.AddSingleton<IContestStore, InMemoryContestStore>();

            return services
                .UseConnectionInitializer<JoinContestInitializer>()
                .UseUpdateFactory<ContestUpdateFactory>()
                .RegisterHandler<StartContest, StartContestHandler>()
                .RegisterHandler<PullRope, PullRopeHandler>();
        }
    }
}
