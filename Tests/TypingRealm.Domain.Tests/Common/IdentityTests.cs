using System;
using TypingRealm.Domain.Common;
using TypingRealm.Domain.Tests.Customizations;
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

        [Theory, AutoDomainData]
        public void ShouldBePrimitive(Identity identity)
        {
            Assert.IsAssignableFrom<Primitive<string>>(identity);
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
