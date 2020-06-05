using System;
using TypingRealm.Domain.Common;
using TypingRealm.Domain.Tests.Customizations;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class PlayerNameTests
    {
        [Theory, AutoDomainData]
        public void ShouldBePrimitive(PlayerName playerName)
        {
            Assert.IsAssignableFrom<Primitive<string>>(playerName);
        }

        [Fact]
        public void ShouldThrow_WhenNameLengthLessThan3()
        {
            Assert.Throws<ArgumentException>(
                () => new PlayerName(new string('a', 2)));

            _ = new PlayerName(new string('a', 3));
        }

        [Fact]
        public void ShouldThrow_WhenNameLengthMoreThan20()
        {
            Assert.Throws<ArgumentException>(
                () => new PlayerName(new string('a', 21)));

            _ = new PlayerName(new string('a', 20));
        }
    }
}
