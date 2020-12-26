using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TypingRealm.Authentication
{
    public static class LocalAuthentication
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler;
        private static readonly SigningCredentials _signingCredentials;

#pragma warning disable S3963, CA1810
        static LocalAuthentication()
        {
            SecurityKey = new SymmetricSecurityKey(new byte[32]);

            _tokenHandler = new JwtSecurityTokenHandler();
            _signingCredentials = new SigningCredentials(
                SecurityKey, SecurityAlgorithms.HmacSha256);
        }
#pragma warning restore S3963, CA1810

        internal static readonly string Issuer = "https://local-authority";
        internal static SecurityKey SecurityKey { get; }

        public static string GenerateProfileAccessToken(string subClaimValue)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", subClaimValue)
            };

            return _tokenHandler.WriteToken(new JwtSecurityToken(Issuer, LocalAuthenticationConfiguration.Audience, claims, null, DateTime.UtcNow.AddMinutes(2), _signingCredentials));
        }

        public static string GenerateServiceAccessToken(string subClaimValue)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", subClaimValue),
                new Claim("scope", "service m2m")
            };

            return _tokenHandler.WriteToken(new JwtSecurityToken(Issuer, LocalAuthenticationConfiguration.Audience, claims, null, DateTime.UtcNow.AddMinutes(2), _signingCredentials));
        }
    }
}
