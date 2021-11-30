using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.Communication
{
    public sealed class AnonymousProfileTokenService : IProfileTokenService
    {
        public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Access tokens are not supported in anonymous provider.");
        }
    }
}
