using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Authentication.Adapters
{
    public sealed class ProfileServiceAdapter : IProfileService
    {
        private readonly IHttpClient _httpClient;

        public ProfileServiceAdapter(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public ValueTask<bool> CharacterBelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken)
        {
            return _httpClient.GetAsync<bool>($"http://host.docker.internal:30103/api/characters/{characterId}/belongsToCurrentProfile", cancellationToken);
        }
    }
}
