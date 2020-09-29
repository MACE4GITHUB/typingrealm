using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public sealed class ProfileTokenService : IProfileTokenService
    {
        private readonly IConnectedClientContext _connectedClientContext;

        public ProfileTokenService(IConnectedClientContext connectedClientContext)
        {
            _connectedClientContext = connectedClientContext;
        }

        public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<string>(_connectedClientContext.GetAccessToken());
        }
    }
}
