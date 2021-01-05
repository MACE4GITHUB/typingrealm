using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Profiles.Api.Client
{
    public sealed class CharactersClient : ICharactersClient
    {
        public static readonly string RoutePrefix = "api/characters";
        private readonly IServiceClient _serviceClient;

        public CharactersClient(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<bool> BelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<bool>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/belongsToCurrentProfile",
                EndpointAuthenticationType.Profile,
                cancellationToken);
        }

        public ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<bool>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/rope-war/{contestId}",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

        public ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _serviceClient.PostAsync<object>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/activity/{activityId}",
                EndpointAuthenticationType.Service,
                new { },
                cancellationToken);
        }
    }
}
