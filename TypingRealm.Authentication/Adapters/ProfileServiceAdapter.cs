using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Authentication.Adapters
{
    public sealed class ProfileServiceAdapter : IProfileService
    {
        private readonly IServiceClient _serviceClient;

        public ProfileServiceAdapter(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<bool> CharacterBelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<bool>("profile", $"characters/{characterId}/belongsToCurrentProfile", cancellationToken);
        }
    }
}
