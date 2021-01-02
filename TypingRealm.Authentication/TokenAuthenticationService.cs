using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public sealed class TokenAuthenticationService : ITokenAuthenticationService
    {
        private readonly IAuthenticationInformationProvider _authenticationInformationProvider;

        public TokenAuthenticationService(IAuthenticationInformationProvider authenticationInformationProvider)
        {
            _authenticationInformationProvider = authenticationInformationProvider;
        }

        public ValueTask<AuthenticationResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken)
        {
            var validationParameters = _authenticationInformationProvider.GetProfileAuthenticationInformation().TokenValidationParameters;

            var handler = new JwtSecurityTokenHandler();
            var user = handler.ValidateToken(accessToken, validationParameters, out var validatedToken);
            if (!(validatedToken is JwtSecurityToken jwtSecurityToken))
                throw new NotSupportedException("Security token is not a valid JWT token.");

            return new ValueTask<AuthenticationResult>(new AuthenticationResult(user, jwtSecurityToken));
        }
    }
}
