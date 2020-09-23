using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Authentication
{
    public sealed class AuthenticateConnectHook : IConnectHook
    {
        private readonly IProfileContext _profileContext;
        private readonly TokenValidationParameters _validationParameters;

        public AuthenticateConnectHook(IProfileContext profileContext, TokenValidationParameters validationParameters)
        {
            _profileContext = profileContext;
            _validationParameters = validationParameters;
        }

        public ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
        {
            var data = connect.ClientId.Split(",");
            var accessToken = data[0];
            var characterId = data[1];

            var handler = new JwtSecurityTokenHandler();
            var user = handler.ValidateToken(accessToken, _validationParameters, out var validatedToken);
            if (!(validatedToken is JwtSecurityToken jwtSecurityToken))
                throw new NotSupportedException("Security token is not a valid JWT token.");

            _profileContext.SetAuthenticatedContext(user, jwtSecurityToken);

            connect.ClientId = characterId;
            return default;
        }
    }
}
