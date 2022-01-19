using System;
using AutoFixture;
using Xunit;

namespace TypingRealm.TextProcessing.Tests
{
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

    public class LanguageInformationTests : TextProcessingTestsBase
    {
        [Theory]
        [InlineData("a abc cabd dabc adbc", "abcd ")]
        [InlineData("a ", "a ")]
        [InlineData("bcd", "dbc")]
        [InlineData("", "")]
        public void ShouldAllowCharacters_WhenAllowed(
            string text, string allowedCharacters)
        {
            var info = Fixture.Build<LanguageInformation>()
                .With(x => x.AllowedCharacters, allowedCharacters)
                .Create();

            Assert.True(info.IsAllLettersAllowed(text));
        }

        [Theory]
        [InlineData("a abc tcb", "abcd ")]
        [InlineData("a ", "a")]
        [InlineData("a", "")]
        [InlineData(" ", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        [InlineData(null, "")]
        [InlineData(null, "abc ")]
        public void ShouldNotAllowCharacters_WhenNotAllowed(
            string text, string allowedCharacters)
        {
            var info = Fixture.Build<LanguageInformation>()
                .With(x => x.AllowedCharacters, allowedCharacters)
                .Create();

            Assert.False(info.IsAllLettersAllowed(text));
        }
    }
}
