using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Authentication
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IProfileTokenService _profileTokenService;
        private readonly IServiceTokenService _serviceTokenService;

        public AccessTokenProvider(
            IProfileTokenService profileTokenService,
            IServiceTokenService serviceTokenService)
        {
            _profileTokenService = profileTokenService;
            _serviceTokenService = serviceTokenService;
        }

        public ValueTask<string> GetProfileTokenAsync(CancellationToken cancellationToken)
        {
            return _profileTokenService.GetProfileAccessTokenAsync(cancellationToken);
        }

        public ValueTask<string> GetServiceTokenAsync(CancellationToken cancellationToken)
        {
            return _serviceTokenService.GetServiceAccessTokenAsync(cancellationToken);
        }
    }
}
