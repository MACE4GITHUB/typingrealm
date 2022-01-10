using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.Communication
{
    public sealed class AnonymousServiceTokenService : IServiceTokenService
    {
        public ValueTask<string> GetServiceAccessTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Access tokens are not supported in anonymous provider.");
        }

        public ValueTask<string> GetServiceAccessTokenAsync(ClientCredentials credentials, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Access tokens are not supported in anonymous provider.");
        }
    }
}
