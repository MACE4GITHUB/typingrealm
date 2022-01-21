using System.Linq;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.TextProcessing.Tests;

public class TextProcessorTests : TextProcessingTestsBase
{
    private readonly TextProcessor _sut;

    public TextProcessorTests()
    {
        _sut = Fixture.Create<TextProcessor>();
    }

    [Fact]
    public void GetSentences_ShouldSplitSentencesByDot_OrQuestion_OrExclamation()
    {
        var text = "Sentence. Another; sentence... One? More# sentences! Some. More";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentence.", sentences[0]);
        Assert.Equal("Another; sentence...", sentences[1]);
        Assert.Equal("One?", sentences[2]);
        Assert.Equal("More# sentences!", sentences[3]);
        Assert.Equal("Some.", sentences[4]);
        Assert.Equal("More.", sentences[5]);
        Assert.Equal(6, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldTrimBeginningAndTheEnd()
    {
        var text = "Sentences.   Another; sentence...    One!  More# sentences !   Some.  ";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentences.", sentences[0]);
        Assert.Equal("Another; sentence...", sentences[1]);
        Assert.Equal("One!", sentences[2]);
        Assert.Equal("More# sentences!", sentences[3]);
        Assert.Equal("Some.", sentences[4]);
        Assert.Equal(5, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldTrimMultipleSpacesInARow()
    {
        var text = " Big   sentence; #   with Some   spaces   !  In   the  middle. ";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Big sentence; # with Some spaces!", sentences[0]);
        Assert.Equal("In the middle.", sentences[1]);
        Assert.Equal(2, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldNotAddAnotherSentence_WhenNoDotAtTheEnd()
    {
        var text = "Sentence one. Sentence two.    ";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentence one.", sentences[0]);
        Assert.Equal("Sentence two.", sentences[1]);
        Assert.Equal(2, sentences.Count); // Not three! Where third is just one dot.
    }

    [Theory]
    [InlineData("“", "\"")]
    [InlineData("”", "\"")]
    [InlineData("«", "\"")]
    [InlineData("»", "\"")]
    [InlineData("’", "'")]
    [InlineData("‘", "'")]
    [InlineData("‚", ",")]
    public void GetSentences_ShouldReplaceUtf8CharactersWithPrintable(
        string symbol, string replacement)
    {
        var text = $"Some {symbol} sentence";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal($"Some {replacement} sentence.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldReplaceUtf8DashBetweenWordsWithPrintable()
    {
        var text = "Some wordy–word and –  with spaces.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some wordy-word and - with spaces.", sentences[0]);
        Assert.Single(sentences);
    }

    // TODO: Consider enhancing sentence generation so that the first letter is a Capital one, even if it goes after - sign.
    [Fact]
    public void GetSentences_ShouldReplaceUtf8LongDashWithPrintable()
    {
        var text = "Some —dash—words sentence — with words.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some - dash - words sentence - with words.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldNotHaveSpaceBeforeFirstLongDash()
    {
        var text = " —Some sentence.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("- Some sentence.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldSupportMultipleDots()
    {
        var text = "Some sentence.. This is it.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some sentence..", sentences[0]);
        Assert.Equal("This is it.", sentences[1]);
        Assert.Equal(2, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldReplaceUtfTripleDotWithPrintable()
    {
        var text = "Sentence one… Sentence two…\" Sentence three…? Sentence four…! Sentence five….. Sentence six…;# Seven";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentence one...", sentences[0]);
        Assert.Equal("Sentence two...\"", sentences[1]);
        Assert.Equal("Sentence three...?", sentences[2]);
        Assert.Equal("Sentence four...!", sentences[3]);
        Assert.Equal("Sentence five.....", sentences[4]);
        Assert.Equal("Sentence six...;#", sentences[5]);
        Assert.Equal("Seven.", sentences[6]);
        Assert.Equal(7, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldReplaceUtfTripleDotBetweenWords_WithoutAddingSpace()
    {
        var text = "Sentence one…this is it.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentence one...this is it.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldAddDotAtTheEndOfEverySentence_WhereNoDotQuestionExclamation()
    {
        var text = "Sentence one! Sentence two? Sentence three";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Sentence one!", sentences[0]);
        Assert.Equal("Sentence two?", sentences[1]);
        Assert.Equal("Sentence three.", sentences[2]);
        Assert.Equal(3, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldMakeFirstLetterCapital()
    {
        var text = " - sentence one;. another sentence?.. another! ";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("- Sentence one;.", sentences[0]);
        Assert.Equal("Another sentence?..", sentences[1]);
        Assert.Equal("Another!", sentences[2]);
        Assert.Equal(3, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldMakeFirstLetterCapital_EvenIfItIsTheLastSymbol()
    {
        var text = " - #~~ -s";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("- #~~ -S.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldReplaceTabWithSpace()
    {
        var text = "\t\t  Something\tis\t up.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Something is up.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldReplaceUtf8SpaceToPrintable()
    {
        var text = "  Some utf8  spaces.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some utf8 spaces.", sentences[0]);
        Assert.Single(sentences);
    }

    [Theory]
    [InlineData("†")]
    public void GetSentences_ShouldRemoveWeirdUtf8Characters(string character)
    {
        var text = $"{character}  Some{character}utf8 {character} spaces{character}.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some utf8 spaces.", sentences[0]);
        Assert.Single(sentences);
    }

    [Fact]
    public void GetSentences_ShouldJoinNewLinesAndRemoveCarriageReturn()
    {
        var text = "\n\r\n\nSome\r\nlines \n of\r text\r\n.\nAnother\nsentence\r\n\n?\nSomething else.";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("Some lines of text.", sentences[0]);
        Assert.Equal("Another sentence?", sentences[1]);
        Assert.Equal("Something else.", sentences[2]);
        Assert.Equal(3, sentences.Count);
    }

    [Fact]
    public void GetSentences_ShouldFilterOutEmptySentences()
    {
        var text = ". . ! ?  - a? ! ... -   ? .. . .abc Sentence one. Two? three";

        var sentences = _sut.GetSentencesEnumerable(text)
            .ToList();

        Assert.Equal("- A?", sentences[0]);
        Assert.Equal("-?", sentences[1]);
        Assert.Equal(".Abc Sentence one.", sentences[2]);
        Assert.Equal("Two?", sentences[3]);
        Assert.Equal("Three.", sentences[4]);
        Assert.Equal(5, sentences.Count);
    }

    // TODO: Make sure when we save words like "#" - raw word is empty and when
    // raw words are requested we need not to pick them up.
    // Mark punctuation-words somehow so they are separate from word-words.
    [Fact]
    public void GetWords_ShouldGetWordsFromText_WithChunksOfPunctuation()
    {
        var text = "  - word # Another; word. another word?   # word";

        var words = _sut.GetWordsEnumerable(text)
            .ToList();

        Assert.Equal("-", words[0]);
        Assert.Equal("Word", words[1]);
        Assert.Equal("#", words[2]);
        Assert.Equal("Another;", words[3]);
        Assert.Equal("word.", words[4]);
        Assert.Equal("Another", words[5]);
        Assert.Equal("word?", words[6]);
        Assert.Equal("#", words[7]);
        Assert.Equal("Word.", words[8]);
    }

    [Fact]
    public void NormalizeWord_ShouldStripPunctuationAroundAndLowercase()
    {
        var text = "  , SOme Word?? $.sOmet hing  ## $.  ";

        var normalized = _sut.NormalizeWord(text);

        Assert.Equal("some word?? $.somet hing", normalized);
    }

    [Fact]
    public void AddTextProcessing_ShouldRegisterAsSingleton()
    {
        var serviceProvider = new ServiceCollection()
            .AddTextProcessing()
            .BuildServiceProvider();

        serviceProvider.AssertRegisteredSingleton<ITextProcessor, TextProcessor>();
    }
}
