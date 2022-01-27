using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

public class IdentityTests
{
    private class TestIdentity : Identity
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
