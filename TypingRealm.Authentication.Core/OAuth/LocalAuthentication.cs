using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TypingRealm.Authentication.OAuth.Configuration;

namespace TypingRealm.Authentication.OAuth
{
    public static class LocalAuthentication
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler;
        private static readonly SigningCredentials _signingCredentials;
        private static readonly LocalAuthenticationConfiguration _authenticationConfiguration
            = new LocalAuthenticationConfiguration();

#pragma warning disable S3963, CA1810
        static LocalAuthentication()
        {
            SecurityKey = new SymmetricSecurityKey(new byte[32]);

            _tokenHandler = new JwtSecurityTokenHandler();
            _signingCredentials = new SigningCredentials(
                SecurityKey, SecurityAlgorithms.HmacSha256);
        }
#pragma warning restore S3963, CA1810

        public static string Issuer => _authenticationConfiguration.Issuer;
        internal static SecurityKey SecurityKey { get; }

        public static string GenerateProfileAccessToken(string subClaimValue)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", subClaimValue)
            };

            return _tokenHandler.WriteToken(new JwtSecurityToken(Issuer, _authenticationConfiguration.Audience, claims, null, DateTime.UtcNow.AddMinutes(60), _signingCredentials));
        }

        public static string GenerateServiceAccessToken()
        {
            var claims = new List<Claim>
            {
                new Claim("scope", TyrScopes.Service)
            };

            return _tokenHandler.WriteToken(new JwtSecurityToken(Issuer, _authenticationConfiguration.Audience, claims, null, DateTime.UtcNow.AddMinutes(60), _signingCredentials));
        }
    }
}
