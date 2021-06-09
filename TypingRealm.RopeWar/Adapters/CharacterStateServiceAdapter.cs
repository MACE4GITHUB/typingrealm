using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.RopeWar.Adapters
{
    // TODO: Consider getting rid of this adapter now that we have ICharactersClient.
    public sealed class CharacterStateServiceAdapter : ICharacterStateService
    {
        private readonly ICharactersClient _charactersClient;

        public CharacterStateServiceAdapter(ICharactersClient charactersClient)
        {
            _charactersClient = charactersClient;
        }

        public ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken)
        {
            return _charactersClient.CanJoinActivityAsync(characterId, contestId, cancellationToken);
        }
    }
}
