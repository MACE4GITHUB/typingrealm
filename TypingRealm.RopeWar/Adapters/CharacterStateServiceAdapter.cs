using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.RopeWar.Adapters
{
    public sealed class CharacterStateServiceAdapter : ICharacterStateService
    {
        private readonly IServiceClient _serviceClient;

        public CharacterStateServiceAdapter(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<bool>("profile", $"characters/{characterId}/rope-war/{contestId}", cancellationToken);
        }
    }
}
