using System.Collections.Generic;
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

        public ValueTask<bool> CanJoinActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<bool>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/activities/{activityId}",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

        public ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _serviceClient.PostAsync<object>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/activities/{activityId}",
                EndpointAuthenticationType.Service,
                new { },
                cancellationToken);
        }

        public ValueTask<Stack<string>> GetActivitiesAsync(string characterId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<Stack<string>>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/activities",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

        public ValueTask LeaveActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _serviceClient.DeleteAsync(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{characterId}/activities/{activityId}",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

    }
}
