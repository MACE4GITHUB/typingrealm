using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Common.Tests;

public class IdentityTests
{
    public class TestIdentity : Identity
    {
        public TestIdentity(string value) : base(value)
        {
        }
    }

    [Theory, AutoMoqData]
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
