using System;
using TypingRealm.Domain.Common;
using Xunit;

namespace TypingRealm.Domain.Tests.Common
{
    public class IdentityTests
    {
        public class TestIdentity : Identity
        {
            public TestIdentity(string value) : base(value)
            {
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("        ")]
        public void ShouldThrowWhenEmpty(string value)
        {
            Assert.Throws<ArgumentException>(
                () => new TestIdentity(value));
        }
    }
}
