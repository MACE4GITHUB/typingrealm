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
}
