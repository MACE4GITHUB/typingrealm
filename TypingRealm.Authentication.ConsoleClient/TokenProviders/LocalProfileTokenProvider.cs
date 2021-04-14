using System.Threading.Tasks;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Authentication.ConsoleClient.TokenProviders
{
    public sealed class LocalProfileTokenProvider : IProfileTokenProvider
    {
        private readonly string _profile;

        public LocalProfileTokenProvider(string profile)
        {
            _profile = profile;
        }

        public ValueTask<string> SignInAsync()
        {
            return new ValueTask<string>(LocalAuthentication.GenerateProfileAccessToken(_profile));
        }
    }
}
