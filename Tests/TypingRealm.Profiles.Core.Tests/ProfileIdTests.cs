using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Profiles.Core.Tests;

public class ProfileIdTests
{
    [Fact]
    public void Anonymous_ShouldCreateAnonymousId()
    {
        Assert.Equal(ProfileId.AnonymousUserId, ProfileId.Anonymous().Value);
    }

    [Fact]
    public void ForService_ShouldCreateServiceId()
    {
        Assert.Equal(ProfileId.ServiceUserId, ProfileId.ForService().Value);
    }

    [Fact]
    public void ForUser_ShouldThrow_WhenUserIdIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => ProfileId.ForUser(null!));
    }

    [Fact]
    public void ForUser_ShouldThrow_WhenUserIdMatchesAnonymousId()
    {
        Assert.Throws<ArgumentException>(
            () => ProfileId.ForUser(ProfileId.AnonymousUserId));
    }

    [Fact]
    public void ForUser_ShouldThrow_WhenUserIdMatchesServiceId()
    {
        Assert.Throws<ArgumentException>(
            () => ProfileId.ForUser(ProfileId.ServiceUserId));
    }

    [Theory, AutoMoqData]
    public void ForUser_ShouldCreateWithUserId(string value)
    {
        Assert.Equal(value, ProfileId.ForUser(value));
    }

    [Fact]
    public void ForUser_ShouldNotAllowEmptyValues()
    {
        Assert.Throws<ArgumentException>(
            () => ProfileId.ForUser("   "));
    }

    [Fact]
    public void ShouldBeIdentity()
    {
        Assert.IsAssignableFrom<Identity>(ProfileId.ForService());
    }
}
