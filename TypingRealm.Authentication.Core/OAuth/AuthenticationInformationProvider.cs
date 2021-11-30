namespace TypingRealm.Authentication.OAuth
{
    public interface IAuthenticationInformationProvider
    {
        AuthenticationInformation GetProfileAuthenticationInformation();
        AuthenticationInformation GetServiceAuthenticationInformation();
    }

    /// <summary>
    /// Stores information used to issue or validate tokens based on requests:
    /// service to service requests use service information, it can be separately
    /// configured from profile information.
    /// </summary>
    public sealed class AuthenticationInformationProvider : IAuthenticationInformationProvider
    {
        private readonly AuthenticationInformation _profileAuthenticationInformation;
        private readonly AuthenticationInformation _serviceAuthenticationInformation;

        public AuthenticationInformationProvider(
            AuthenticationInformation profileAuthenticationInformation,
            AuthenticationInformation serviceAuthenticationInformation)
        {
            _profileAuthenticationInformation = profileAuthenticationInformation;
            _serviceAuthenticationInformation = serviceAuthenticationInformation;
        }

        public AuthenticationInformation GetProfileAuthenticationInformation()
        {
            return _profileAuthenticationInformation;
        }

        public AuthenticationInformation GetServiceAuthenticationInformation()
        {
            return _serviceAuthenticationInformation;
        }
    }
}
