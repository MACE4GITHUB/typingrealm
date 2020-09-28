using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication.Adapters
{
    // TODO: Consider getting rid of this adapter now that we have ICharactersClient.
    public sealed class ProfileServiceAdapter : IProfileService
    {
        private readonly ICharactersClient _charactersClient;

        public ProfileServiceAdapter(ICharactersClient charactersClient)
        {
            _charactersClient = charactersClient;
        }

        public ValueTask<bool> CharacterBelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken)
        {
            return _charactersClient.BelongsToCurrentProfileAsync(characterId, cancellationToken);
        }
    }
}
