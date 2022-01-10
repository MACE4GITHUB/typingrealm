using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;
using TypingRealm.Profiles.Api.Resources;
using TypingRealm.Profiles.Api.Resources.Data;

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
                EndpointAuthentication.Profile,
                cancellationToken);
        }

        public ValueTask CreateAsync(CreateCharacterDto dto, CancellationToken cancellationToken)
        {
            return _serviceClient.PostAsync(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}",
                EndpointAuthentication.Profile,
                dto,
                cancellationToken);
        }

        public ValueTask<IEnumerable<CharacterResource>> GetAllByProfileIdAsync(CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<IEnumerable<CharacterResource>>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}",
                EndpointAuthentication.Profile,
                cancellationToken);
        }
    }
}
