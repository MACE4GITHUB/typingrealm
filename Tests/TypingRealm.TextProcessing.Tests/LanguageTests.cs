using System;
using Xunit;

namespace TypingRealm.TextProcessing.Tests;

public class LanguageTests
{
    [Fact]
    public void ShouldSupportAllSupportedLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var sut = new Language(languageValue);
            Assert.Equal(languageValue, sut.Value);
        }
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("some")]
    [InlineData("language")]
    [InlineData("")]
    public void ShouldNotSupportAnyWrongValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new Language(value));
    }

    [Fact]
    public void ShouldThrow_WhenNullIsPassed()
    {
        Assert.Throws<ArgumentNullException>(() => new Language(null!));
    }
}
