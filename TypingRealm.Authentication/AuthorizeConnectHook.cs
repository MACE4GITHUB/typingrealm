using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Authentication
{
    public sealed class AuthorizeConnectHook : IConnectHook
    {
        private readonly IProfileService _profileService;

        public AuthorizeConnectHook(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public async ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
        {
            var characterBelongsToCurrentProfile = await _profileService.CharacterBelongsToCurrentProfileAsync(connect.ClientId, cancellationToken)
                .ConfigureAwait(false);

            if (!characterBelongsToCurrentProfile)
                throw new UnauthorizedAccessException($"Character {connect.ClientId} does not belong to current profile.");
        }
    }
}
