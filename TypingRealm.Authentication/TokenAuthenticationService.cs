using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TypingRealm.Authentication
{
    public sealed class TokenAuthenticationService : ITokenAuthenticationService
    {
        private readonly TokenValidationParameters _validationParameters;

        public TokenAuthenticationService(TokenValidationParameters validationParameters)
        {
            _validationParameters = validationParameters;
        }

        public ValueTask<AuthenticationResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var user = handler.ValidateToken(accessToken, _validationParameters, out var validatedToken);
            if (!(validatedToken is JwtSecurityToken jwtSecurityToken))
                throw new NotSupportedException("Security token is not a valid JWT token.");

            return new ValueTask<AuthenticationResult>(new AuthenticationResult(user, jwtSecurityToken));
        }
    }
}
