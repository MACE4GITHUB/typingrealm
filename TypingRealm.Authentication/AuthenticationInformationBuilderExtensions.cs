using System.Threading;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TypingRealm.Authentication
{
    public static class TyrAuthenticationSchemes
    {
        public static readonly string ProfileAuthenticationScheme = "ProfileAuthenticationScheme";
        public static readonly string ServiceAuthenticationScheme = "ServiceAuthenticationScheme";
    }

    public static class AuthenticationInformationBuilderExtensions
    {
        public static AuthenticationInformationBuilder UseAuth0Provider(this AuthenticationInformationBuilder builder)
        {
            var parameters = builder.AuthenticationInformation.TokenValidationParameters;

            builder.AuthenticationInformation.Issuer = Auth0AuthenticationConfiguration.Issuer;
            builder.AuthenticationInformation.AuthorizationEndpoint = Auth0AuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = Auth0AuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = Auth0AuthenticationConfiguration.ServiceClientId;
            builder.AuthenticationInformation.ServiceClientSecret = Auth0AuthenticationConfiguration.ServiceClientSecret;

            parameters.ValidAudiences = new[] { Auth0AuthenticationConfiguration.Audience };
            parameters.ValidIssuer = Auth0AuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{parameters.ValidIssuer}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

                return openIdConfig.SigningKeys;
            };

            return builder;
        }

        public static AuthenticationInformationBuilder UseIdentityServerProvider(this AuthenticationInformationBuilder builder)
        {
            var parameters = builder.AuthenticationInformation.TokenValidationParameters;

            builder.AuthenticationInformation.Issuer = IdentityServerAuthenticationConfiguration.Issuer;
            builder.AuthenticationInformation.AuthorizationEndpoint = IdentityServerAuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = IdentityServerAuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = IdentityServerAuthenticationConfiguration.ServiceClientId;
            builder.AuthenticationInformation.ServiceClientSecret = IdentityServerAuthenticationConfiguration.ServiceClientSecret;

            parameters.ValidAudiences = new[] { IdentityServerAuthenticationConfiguration.Audience };
            parameters.ValidIssuer = IdentityServerAuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{parameters.ValidIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

                return openIdConfig.SigningKeys;
            };

            return builder;
        }

        public static AuthenticationInformationBuilder UseLocalProvider(this AuthenticationInformationBuilder builder)
        {
            var parameters = builder.AuthenticationInformation.TokenValidationParameters;

            builder.AuthenticationInformation.AuthorizationEndpoint = LocalAuthenticationConfiguration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = LocalAuthenticationConfiguration.TokenEndpoint;

            builder.AuthenticationInformation.ServiceClientId = "local-service";
            builder.AuthenticationInformation.ServiceClientSecret = "local-secret";

            parameters.ValidAudiences = new[] { LocalAuthenticationConfiguration.Audience };
            parameters.ValidIssuer = LocalAuthenticationConfiguration.Issuer;
            parameters.IssuerSigningKeyResolver = null;
            parameters.IssuerSigningKey = LocalAuthentication.SecurityKey;

            return builder;
        }
    }
}
