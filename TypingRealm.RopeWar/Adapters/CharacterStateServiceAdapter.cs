using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.RopeWar.Adapters
{
    public sealed class CharacterStateServiceAdapter : ICharacterStateService
    {
        private readonly IHttpClient _httpClient;

        public CharacterStateServiceAdapter(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken)
        {
            return _httpClient.GetAsync<bool>($"http://host.docker.internal:30103/api/characters/{characterId}/rope-war/{contestId}", cancellationToken);
        }
    }
}
