using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Authentication
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IProfileTokenService _profileTokenService;

        public AccessTokenProvider(IProfileTokenService profileTokenService)
        {
            _profileTokenService = profileTokenService;
        }

        public ValueTask<string> GetProfileTokenAsync(CancellationToken cancellationToken)
        {
            return _profileTokenService.GetProfileAccessTokenAsync(cancellationToken);
        }

        public ValueTask<string> GetServiceTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
