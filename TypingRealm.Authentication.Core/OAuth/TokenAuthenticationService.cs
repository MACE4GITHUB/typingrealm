using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication.OAuth
{
    public interface ITokenAuthenticationService
    {
        /// <summary>
        /// Validates JWT access token and creates Principal & Security information.
        /// Used to work only for Profile token, now it also works for service tokens.
        /// </summary>
        ValueTask<AuthenticationResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken);
    }

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
            {
                var serviceValidationParameters = _authenticationInformationProvider.GetServiceAuthenticationInformation().TokenValidationParameters;
                user = handler.ValidateToken(accessToken, serviceValidationParameters, out validatedToken);
                if (!(validatedToken is JwtSecurityToken))
                    throw new NotSupportedException("Security token is not a valid JWT token.");

                jwtSecurityToken = (JwtSecurityToken)validatedToken;
            }

            return new ValueTask<AuthenticationResult>(new AuthenticationResult(user, jwtSecurityToken));
        }
    }
}
