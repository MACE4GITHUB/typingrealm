using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Authentication.ConsoleClient
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IProfileTokenProvider _profileTokenProvider;

        public AccessTokenProvider(IProfileTokenProvider profileTokenProvider)
        {
            _profileTokenProvider = profileTokenProvider;
        }

        public ValueTask<string> GetProfileTokenAsync(CancellationToken cancellationToken)
        {
            return _profileTokenProvider.SignInAsync(cancellationToken);
        }

        public ValueTask<string> GetServiceTokenAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Console client cannot call service endpoints directly.");
        }
    }
}
