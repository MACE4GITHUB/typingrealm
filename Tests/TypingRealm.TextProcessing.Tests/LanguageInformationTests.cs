using System;
using Xunit;

namespace TypingRealm.TextProcessing.Tests
{
    public class LanguageInformationTests : TextProcessingTestsBase
    {
        [Theory]
        [InlineData("a abc cabd dabc adbc", "abcd ")]
        [InlineData("a ", "a ")]
        [InlineData("bcd", "dbc")]
        [InlineData("", "dbc")]
        public void ShouldAllowCharacters_WhenAllowed(
            string text, string allowedCharacters)
        {
            var info = Create<LanguageInformation>(
                new LanguageInformationBuilder(allowedCharacters));

            Assert.True(info.IsAllLettersAllowed(text));
        }

        [Theory]
        [InlineData("a abc tcb", "abcd ")]
        [InlineData("a ", "a")]
        public void ShouldNotAllowCharacters_WhenNotAllowed(
            string text, string allowedCharacters)
        {
            var info = Create<LanguageInformation>(
                new LanguageInformationBuilder(allowedCharacters));

            Assert.False(info.IsAllLettersAllowed(text));
        }

        [Theory]
        [AutoDomainData]
        public void IsAllLettersAllowed_ShouldThrow_WhenInputIsNull(
            LanguageInformation info)
        {
            Assert.Throws<ArgumentNullException>(() => info.IsAllLettersAllowed(null!));
        }

        [Theory, AutoDomainData]
        public void ShouldThrow_WhenAllowedCharactersAreEmpty(Language language)
        {
            Assert.Throws<ArgumentException>(
                () => new LanguageInformation(language, string.Empty));
        }

        [Theory, AutoDomainData]
        public void ShouldThrow_WhenAllowedCharactersAreNull(Language language)
        {
            Assert.Throws<ArgumentNullException>(
                () => new LanguageInformation(language, null!));
        }

        [Theory, AutoDomainData]
        public void ShouldThrow_WhenLanguageIsNull(string allowedCharacters)
        {
            Assert.Throws<ArgumentNullException>(
                () => new LanguageInformation(null!, allowedCharacters));
        }
    }
}
