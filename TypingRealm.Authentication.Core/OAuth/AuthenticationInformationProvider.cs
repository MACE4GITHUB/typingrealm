using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Authentication.OAuth;

public interface IAuthenticationInformationProvider
{
    AuthenticationInformation GetProfileAuthenticationInformation();
    IEnumerable<AuthenticationInformation> GetAdditionalProfileAuthenticationInformations();
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
    private readonly IEnumerable<AuthenticationInformation> _additionalProfileAuthenticationInformations
        = Enumerable.Empty<AuthenticationInformation>();
    private readonly AuthenticationInformation _serviceAuthenticationInformation;

    public AuthenticationInformationProvider(
        AuthenticationInformation profileAuthenticationInformation,
        AuthenticationInformation serviceAuthenticationInformation)
    {
        _profileAuthenticationInformation = profileAuthenticationInformation;
        _serviceAuthenticationInformation = serviceAuthenticationInformation;
    }

    public AuthenticationInformationProvider(
        AuthenticationInformation profileAuthenticationInformation,
        IEnumerable<AuthenticationInformation> additionalProfileAuthenticationInformations,
        AuthenticationInformation serviceAuthenticationInformation)
    {
        if (_additionalProfileAuthenticationInformations.Contains(profileAuthenticationInformation))
            throw new InvalidOperationException("Additional profile authentication informations cannot contain primary entry.");

        _profileAuthenticationInformation = profileAuthenticationInformation;
        _additionalProfileAuthenticationInformations = additionalProfileAuthenticationInformations;
        _serviceAuthenticationInformation = serviceAuthenticationInformation;
    }

    public AuthenticationInformation GetProfileAuthenticationInformation()
    {
        return _profileAuthenticationInformation;
    }

    public IEnumerable<AuthenticationInformation> GetAdditionalProfileAuthenticationInformations()
    {
        return _additionalProfileAuthenticationInformations;
    }

    public AuthenticationInformation GetServiceAuthenticationInformation()
    {
        return _serviceAuthenticationInformation;
    }
}
