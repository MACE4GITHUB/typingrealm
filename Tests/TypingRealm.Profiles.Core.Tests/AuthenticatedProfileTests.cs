using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Profiles.Core.Tests;

public class AuthenticatedProfileTests : TestsBase
{
    [Fact]
    public void AnonymousProfile_ShouldHaveAnonymousType()
    {
        var sut = AuthenticatedProfile.Anonymous();

        Assert.Equal(ProfileType.Anonymous, sut.Type);
    }

    [Fact]
    public void AnonymousProfile_ShouldNotBeAuthenticated()
    {
        var sut = AuthenticatedProfile.Anonymous();

        Assert.False(sut.IsAuthenticated);
    }

    [Fact]
    public void AnonymousProfile_ShouldHaveAnonymousProfileId()
    {
        var sut = AuthenticatedProfile.Anonymous();

        Assert.Equal(ProfileId.AnonymousUserId, sut.ProfileId);
    }

    [Theory, AutoMoqData]
    public void UserProfile_ShouldHaveAnonymousType_WhenNotAuthenticated(string sub)
    {
        var principal = NewPrincipal(sub, false);
        Assert.False(principal.Identity!.IsAuthenticated);

        var sut = AuthenticatedProfile.GetProfileForUser(principal);

        Assert.Equal(ProfileType.Anonymous, sut.Type);
    }

    [Fact]
    public void ServiceProfile_ShouldHaveAnonymousType_WhenNotAuthenticated()
    {
        var principal = NewPrincipal(null, false);
        Assert.False(principal.Identity!.IsAuthenticated);

        var sut = AuthenticatedProfile.GetProfileForUser(principal);

        Assert.Equal(ProfileType.Anonymous, sut.Type);
    }

    [Theory, AutoMoqData]
    public void UserProfile_ShouldHaveUserType_WhenAuthenticated(string sub)
    {
        var principal = NewPrincipal(sub, true);
        Assert.True(principal.Identity!.IsAuthenticated);

        var sut = AuthenticatedProfile.GetProfileForUser(principal);

        Assert.Equal(ProfileType.User, sut.Type);
    }

    [Fact]
    public void ServiceProfile_ShouldHaveServiceType_WhenAuthenticated()
    {
        var sut = AuthenticatedProfile.ForService();

        Assert.Equal(ProfileType.Service, sut.Type);
    }

    [Fact]
    public void UserProfile_ShouldReturnServiceType_WhenAuthenticated_AndSubIsNull()
    {
        var principal = NewPrincipal(null, true);
        Assert.True(principal.Identity!.IsAuthenticated);

        var sut = AuthenticatedProfile.GetProfileForUser(principal);

        Assert.Equal(ProfileType.Service, sut.Type);
    }

    [Fact]
    public void UserProfile_ShouldThrow_WhenClaimsPrincipalIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => AuthenticatedProfile.GetProfileForUser(null!));
    }
}
