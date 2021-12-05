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
            var authInfo = _authenticationInformationProvider.GetProfileAuthenticationInformation();
            var result = TryValidateToken(accessToken, authInfo);
            if (result != null)
                return new ValueTask<AuthenticationResult>(result);

            foreach (var additionalAuthInfo in _authenticationInformationProvider.GetAdditionalProfileAuthenticationInformations())
            {
                result = TryValidateToken(accessToken, additionalAuthInfo);
                if (result != null)
                    return new ValueTask<AuthenticationResult>(result);
            }

            var serviceAuthInfo = _authenticationInformationProvider.GetServiceAuthenticationInformation();
            result = TryValidateToken(accessToken, serviceAuthInfo);
            if (result != null)
                return new ValueTask<AuthenticationResult>(result);

            throw new NotSupportedException("Security token is not a valid JWT token.");
        }

        private static AuthenticationResult? TryValidateToken(string accessToken, AuthenticationInformation authenticationInformation)
        {
            var handler = new JwtSecurityTokenHandler();
            var user = handler.ValidateToken(
                accessToken,
                authenticationInformation.TokenValidationParameters,
                out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
                return new AuthenticationResult(user, jwtSecurityToken);

            return null;
        }
    }
}
