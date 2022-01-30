using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication.Service;

public sealed class AuthorizeCharacterConnectHook : IConnectHook
{
    private readonly ICharactersClient _charactersClient;

    public AuthorizeCharacterConnectHook(ICharactersClient charactersClient)
    {
        _charactersClient = charactersClient;
    }

    public async ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
    {
        var characterBelongsToCurrentProfile = await _charactersClient.BelongsToCurrentProfileAsync(connect.ClientId, cancellationToken)
            .ConfigureAwait(false);

        if (!characterBelongsToCurrentProfile)
            throw new UnauthorizedAccessException($"Character {connect.ClientId} does not belong to current profile.");
    }
}
