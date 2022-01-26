using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

public class ValidationTests : TestsBase
{
    [Theory]
    [InlineData("value", 0, 4)]
    [InlineData("", 1, 4)]
    [InlineData("value", 6, 8)]
    [InlineData("value", 6, 6)]
    public void ValidateLength_Error(string value, int minLength, int maxLength)
    {
        Assert.Throws<ArgumentException>(
            () => Validation.ValidateLength(value, minLength, maxLength));
    }

    [Theory]
    [InlineData("value", 0, 5)]
    [InlineData("value", 5, 10)]
    [InlineData("value", 2, 8)]
    [InlineData("", 0, 5)]
    [InlineData("", 0, 0)]
    [InlineData("value", 5, 5)]
    public void ValidateLength_Successful(string value, int minLength, int maxLength)
    {
        Validation.ValidateLength(value, minLength, maxLength);
    }
}
