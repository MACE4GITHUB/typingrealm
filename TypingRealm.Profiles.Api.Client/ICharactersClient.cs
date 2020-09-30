﻿using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Api.Client
{
    public interface ICharactersClient
    {
        ValueTask<bool> BelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken);

        ValueTask<bool> CanJoinRopeWarContest(string characterId, string contestId, CancellationToken cancellationToken);
    }
}