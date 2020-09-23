using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public sealed class ProfileTokenService : IProfileTokenService
    {
        private readonly IProfileContext _profileContext;

        public ProfileTokenService(IProfileContext profileContext)
        {
            _profileContext = profileContext;
        }

        public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<string>(_profileContext.GetAccessToken());
        }
    }
}
