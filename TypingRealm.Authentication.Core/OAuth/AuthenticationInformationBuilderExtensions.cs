using System.Threading;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TypingRealm.Authentication.OAuth.Configuration;

namespace TypingRealm.Authentication.OAuth
{
    public static class AuthenticationInformationBuilderExtensions
    {
        public static AuthenticationInformationBuilder UseAuthenticationProvider(
            this AuthenticationInformationBuilder builder,
            AuthenticationProviderConfiguration configuration)
        {
            var parameters = builder.AuthenticationInformation.TokenValidationParameters;

            builder.AuthenticationInformation.Issuer = configuration.Issuer;
            builder.AuthenticationInformation.AuthorizationEndpoint = configuration.AuthorizationEndpoint;
            builder.AuthenticationInformation.TokenEndpoint = configuration.TokenEndpoint;

            builder.AuthenticationInformation.RequireHttpsMetadata = configuration.RequireHttpsMetadata;

            builder.AuthenticationInformation.ServiceClientId = configuration.ServiceClientId;
            builder.AuthenticationInformation.ServiceClientSecret = configuration.ServiceClientSecret;

            parameters.ValidAudiences = new[] { configuration.Audience };
            parameters.ValidIssuer = configuration.Issuer;
            parameters.IssuerSigningKey = null;
            parameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{parameters.ValidIssuer}.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever
                    {
                        RequireHttps = configuration.RequireHttpsMetadata
                    });

                var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

                return openIdConfig.SigningKeys;
            };

            return builder;
        }

        public static AuthenticationInformationBuilder UseAuth0Provider(this AuthenticationInformationBuilder builder, bool isDevelopment = false)
        {
            if (isDevelopment)
                return builder.UseAuthenticationProvider(new DevelopmentAuth0AuthenticationConfiguration());

            return builder.UseAuthenticationProvider(new Auth0AuthenticationConfiguration());
        }

        public static AuthenticationInformationBuilder UseIdentityServerProvider(this AuthenticationInformationBuilder builder)
        {
            return builder.UseAuthenticationProvider(new IdentityServerAuthenticationConfiguration());
        }

        public static AuthenticationInformationBuilder UseLocalProvider(this AuthenticationInformationBuilder builder)
        {
            builder.UseAuthenticationProvider(new LocalAuthenticationConfiguration());

            var parameters = builder.AuthenticationInformation.TokenValidationParameters;

            parameters.IssuerSigningKeyResolver = null;
            parameters.IssuerSigningKey = LocalAuthentication.SecurityKey;

            return builder;
        }
    }
}
