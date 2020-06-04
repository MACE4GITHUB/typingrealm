using TypingRealm.Domain.Common;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class PlayerIdTests
    {
        [Theory, AutoDomainData]
        public void ShouldBeIdentity(PlayerId playerId)
        {
            Assert.IsAssignableFrom<Identity>(playerId);
        }
    }
}
