using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public sealed class AnonymousAccessTokenProvider : IAccessTokenProvider
    {
        public ValueTask<string> GetProfileTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Access tokens are not supported in anonymous provider.");
        }

        public ValueTask<string> GetServiceTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Access tokens are not supported in anonymous provider.");
        }
    }
}
