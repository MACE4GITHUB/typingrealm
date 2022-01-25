using TypingRealm.Common;
using TypingRealm.Domain.Tests.Customizations;
using Xunit;

namespace TypingRealm.Domain.Tests;

public class PlayerIdTests
{
    [Theory, AutoDomainData]
    public void ShouldBeIdentity(PlayerId playerId)
    {
        Assert.IsAssignableFrom<Identity>(playerId);
    }
}
