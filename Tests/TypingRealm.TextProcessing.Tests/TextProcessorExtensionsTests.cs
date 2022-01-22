using System.Linq;
using AutoFixture;
using Xunit;

namespace TypingRealm.TextProcessing.Tests;

public class TextProcessorExtensionsTests : TextProcessingTestsBase
{
    private readonly TextProcessor _sut;

    public TextProcessorExtensionsTests()
    {
        _sut = Fixture.Create<TextProcessor>();
    }

    [Fact]
    public void GetSentences_ShouldGetOnlySentencesWithAllowedCharacters()
    {
        var text = "abcd bc. ba   cd? abcd? bcde";

        var languageInfo = Create<LanguageInformation>(
            new LanguageInformationBuilder("Aabcd ?."));

        var sentences = _sut.GetSentencesEnumerable(text, languageInfo)
            .ToList();

        Assert.Equal("Abcd bc.", sentences[0]);
        Assert.Equal("Abcd?", sentences[1]);
        Assert.Equal(2, sentences.Count);
    }

    [Fact]
    public void GetNotAllowedSentences_ShouldGetOnlySentencesWithNotAllowedCharacters()
    {
        var text = "abcd bc. ba   cd? abcd? bcde";

        var languageInfo = Create<LanguageInformation>(
            new LanguageInformationBuilder("Aabcd ?."));

        var sentences = _sut.GetNotAllowedSentencesEnumerable(text, languageInfo)
            .ToList();

        Assert.Equal("Ba cd?", sentences[0]);
        Assert.Equal("Bcde.", sentences[1]);
        Assert.Equal(2, sentences.Count);
    }

    [Fact]
    public void GetWords_ShouldGetOnlyWordsWithAllowedCharacters_NonUnique()
    {
        var text = " abc. ab abc abcd! abc end. abc end.";

        var languageInfo = Create<LanguageInformation>(
            new LanguageInformationBuilder("Aabc"));

        var words = _sut.GetWordsEnumerable(text, languageInfo)
            .ToList();

        Assert.Equal("Ab", words[0]);
        Assert.Equal("abc", words[1]);
        Assert.Equal("Abc", words[2]);
        Assert.Equal("Abc", words[3]);
        Assert.Equal(4, words.Count);
    }

    [Fact]
    public void GetNormalizedWords_ShohuldGetNormalizedWords_Unique()
    {
        var text = " abC. Ab aBc ABCD! abc end. abc end. #,.";

        var words = _sut.GetNormalizedWordsEnumerable(text)
            .ToList();

        Assert.Equal("abc", words[0]);
        Assert.Equal("ab", words[1]);
        Assert.Equal("abcd", words[2]);
        Assert.Equal("end", words[3]);
        Assert.Equal(4, words.Count);
    }

    [Fact]
    public void GetNormalizedWords_ShouldGetNormalizedWords_Unique_AndWithinAllowedCharactersRange()
    {
        var text = " abC. Ab aBc ABCD! abc end. abc end. #,.";

        var languageInfo = Create<LanguageInformation>(
            new LanguageInformationBuilder("Aabc.!#, "));

        var words = _sut.GetNormalizedWordsEnumerable(text, languageInfo)
            .ToList();

        Assert.Equal("abc", words[0]);
        Assert.Equal("ab", words[1]);
        Assert.Equal(2, words.Count);
    }
}
