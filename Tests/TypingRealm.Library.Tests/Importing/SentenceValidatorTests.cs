using System;
using TypingRealm.Library.Importing;
using Xunit;

namespace TypingRealm.Library.Tests.Importing;

public sealed class SentenceValidatorTests : LibraryTestsBase
{
    private readonly SentenceValidator _sut;

    public SentenceValidatorTests()
    {
        _sut = Create<SentenceValidator>();
    }

    [Fact]
    public void ShouldThrow_WhenNullIsPassed()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.IsValidSentence(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("abcdefg")]
    public void ShouldInvalidateShortSentences(string sentence)
    {
        Assert.False(_sut.IsValidSentence(sentence));
    }

    [Theory]
    [InlineData("abcdefgh")]
    [InlineData("some oth")]
    [InlineData("some other sentence")]
    public void ShouldValidateSentencesOfAtLeast8CharactersLength(string sentence)
    {
        Assert.True(_sut.IsValidSentence(sentence));
    }

    [Theory]
    [InlineData("Apple was gOOd!")]
    [InlineData("ANother sENTENCE.")]
    [InlineData("CAPsLock")]
    public void ShouldInvalidate_WhenMultipleUppercaseCharactersInARow(string sentence)
    {
        Assert.False(_sut.IsValidSentence(sentence));
    }

    [Theory]
    [InlineData("Apple was gOod!")]
    [InlineData("AnOtHeR Abc.")]
    [InlineData("CaCaaNnNnN")]
    public void ShouldValidate_WhenNoMultipleUppercaseCharactersInARow(string sentence)
    {
        Assert.True(_sut.IsValidSentence(sentence));
    }

    [Theory]
    //[InlineData("            ")]
    [InlineData("  .   .#1343  ~~~2341-/=\"")]
    [InlineData("### -- ===\\///")]
    public void ShouldInvalidate_WhenAllCharactersArePunctuationOrNumbers(string sentence)
    {
        Assert.False(_sut.IsValidSentence(sentence));
    }

    [Theory]
    //[InlineData("       m     ")]
    [InlineData("  .   .#1343  a~~~2341-/=\"")]
    [InlineData("#l## -- ===\\///")]
    public void ShouldValidate_WhenAtLeastOneSymbol(string sentence)
    {
        Assert.True(_sut.IsValidSentence(sentence));
    }

    [Theory]
    [InlineData(" abcdefg", false)]
    [InlineData("           m      ", false)]
    [InlineData("       a     b    ", false)]
    [InlineData("       a      b    ", true)]
    public void ShouldCheckTheLengthOfTrimmedSentence(string sentence, bool shouldBeValid)
    {
        if (shouldBeValid)
            Assert.True(_sut.IsValidSentence(sentence));
        else
            Assert.False(_sut.IsValidSentence(sentence));
    }
}
