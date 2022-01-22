using System;
using System.Linq;
using AutoFixture;
using Moq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Importing;
using TypingRealm.TextProcessing;
using Xunit;

namespace TypingRealm.Library.Tests.Importing
{
    public class SentenceFactoryTests : LibraryTestsBase
    {
        private readonly Mock<ITextProcessor> _textProcessor;
        private readonly SentenceFactory _sut;

        public SentenceFactoryTests()
        {
            _textProcessor = Freeze<ITextProcessor>();
            _sut = Fixture.Create<SentenceFactory>();
        }

        [Theory, AutoDomainData]
        public void ShouldCreateSentence_WithValidMetadata_EndToEnd(BookId bookId, int indexInBook)
        {
            Fixture.Register<ITextProcessor>(() => new TextProcessor());
            var e2eSut = Fixture.Create<SentenceFactory>();

            var text = "Simple validated, $ validated - validated Simple; sentENce?";
            var sentence = e2eSut.CreateSentence(bookId, text, indexInBook);

            Assert.False(string.IsNullOrWhiteSpace(sentence.SentenceId));
            Assert.NotEqual(Guid.Empty, Guid.Parse(sentence.SentenceId));
            Assert.Equal(bookId, sentence.BookId);
            Assert.Equal(indexInBook, sentence.IndexInBook);
            Assert.Equal(text, sentence.Value);

            var words = sentence.Words.ToList();
            Assert.True(words.All(word => sentence.SentenceId == word.SentenceId));

            Assert.Equal("Simple", words[0].Value);
            Assert.Equal(1, words[0].CountInSentence);
            Assert.Equal("simple", words[0].RawValue);
            Assert.Equal(2, words[0].RawCountInSentence);
            Assert.Equal("validated,", words[1].Value);
            Assert.Equal(1, words[1].CountInSentence);
            Assert.Equal("validated", words[1].RawValue);
            Assert.Equal(3, words[1].RawCountInSentence);
            Assert.Equal("$", words[2].Value);
            Assert.Equal(1, words[2].CountInSentence);
            Assert.Equal("", words[2].RawValue);
            Assert.Equal(0, words[2].RawCountInSentence);
            Assert.Equal("validated", words[3].Value);
            Assert.Equal(2, words[3].CountInSentence);
            Assert.Equal("validated", words[3].RawValue);
            Assert.Equal(3, words[3].RawCountInSentence);
            Assert.Equal("-", words[4].Value);
            Assert.Equal(1, words[4].CountInSentence);
            Assert.Equal("", words[4].RawValue);
            Assert.Equal(0, words[4].RawCountInSentence);
            Assert.Equal("validated", words[5].Value);
            Assert.Equal(2, words[5].CountInSentence);
            Assert.Equal("validated", words[5].RawValue);
            Assert.Equal(3, words[5].RawCountInSentence);
            Assert.Equal("Simple;", words[6].Value);
            Assert.Equal(1, words[6].CountInSentence);
            Assert.Equal("simple", words[6].RawValue);
            Assert.Equal(2, words[6].RawCountInSentence);
            Assert.Equal("sentENce?", words[7].Value);
            Assert.Equal(1, words[7].CountInSentence);
            Assert.Equal("sentence", words[7].RawValue);
            Assert.Equal(1, words[7].RawCountInSentence);

            var keyPairs = words[1].KeyPairs.ToList();

            Assert.Equal(" v", keyPairs[0].Value);
            Assert.Equal(" va", keyPairs[1].Value);

            var pairs = "va val al ali li lid id ida da dat at ate te ted ed ed, d,";
            Assert.Equal(pairs, string.Join(" ", keyPairs.Skip(2).Take(keyPairs.Count - 4).Select(kp => kp.Value)));

            Assert.Equal("d, ", keyPairs[^2].Value);
            Assert.Equal(", ", keyPairs[^1].Value);

            Assert.True(keyPairs.All(x => x.CountInWord == 1));
            Assert.Equal(3, keyPairs[1].CountInSentence);
            Assert.Equal(3, keyPairs[4].CountInSentence);
            Assert.True(keyPairs.Take(keyPairs.Count - 4).All(x => x.CountInSentence == 3));
            Assert.True(keyPairs.Skip(keyPairs.Count - 4).All(x => x.CountInSentence == 1));

            Assert.Equal(-1, keyPairs[0].IndexInWord);
            Assert.Equal(-1, keyPairs[1].IndexInWord);
            Assert.Equal(0, keyPairs[2].IndexInWord);
            Assert.Equal(0, keyPairs[3].IndexInWord);

            var index = -1;
            var skip = true;
            foreach (var keyPair in keyPairs)
            {
                Assert.Equal(index, keyPair.IndexInWord);

                if (skip)
                {
                    skip = false;
                    continue;
                }

                else
                {
                    skip = true;
                    index++;
                }
            }
        }

        [Theory, AutoDomainData]
        public void ShouldCreateSentence_WithWeirdFormat_EndToEnd(BookId bookId, int indexInBook)
        {
            Fixture.Register<ITextProcessor>(() => new TextProcessor());
            var e2eSut = Fixture.Create<SentenceFactory>();

            var text = "  -$ Simple sentence ... . soME other sentence. # , . ! ? another; sentence-  ";
            var sentence = e2eSut.CreateSentence(bookId, text, indexInBook);

            Assert.Equal("-$ Simple sentence ... SoME other sentence. # ,. Another; sentence-.", sentence.Value);
            Assert.Equal("-$", sentence.Words.First().Value);
            Assert.Equal("...", sentence.Words.ToList()[3].Value);
            Assert.Equal(" .| ..|..|...|..|.. |. ", string.Join("|", sentence.Words.ToList()[3].KeyPairs.Select(kp => kp.Value)));
        }

        [Theory, AutoDomainData]
        public void ShouldCountKeyPairsInWord_EndToEnd(BookId bookId, int indexInBook)
        {
            Fixture.Register<ITextProcessor>(() => new TextProcessor());
            var e2eSut = Fixture.Create<SentenceFactory>();

            var text = "sasasasa, sentence...";
            var sentence = e2eSut.CreateSentence(bookId, text, indexInBook);

            var word = sentence.Words.First();
            Assert.Equal("Sasasasa,", word.Value);

            var keyPairs = word.KeyPairs.ToList();

            Assert.True(word.KeyPairs.Where(kp => kp.Value == "sa").All(kp => kp.CountInWord == 3));
        }

        [Theory, AutoDomainData]
        public void ShouldCreateSentence_WhenMultipleSentences_EndToEnd(
            BookId bookId, int indexInBook)
        {
            Fixture.Register<ITextProcessor>(() => new TextProcessor());
            var e2eSut = Fixture.Create<SentenceFactory>();

            var text = " sentence, one.  sentence two?..";

            var sentence = e2eSut.CreateSentence(bookId, text, indexInBook);
            var words = sentence.Words.ToList();

            Assert.Equal("Sentence, one. Sentence two?..", sentence.Value);
            Assert.Equal("Sentence,", words[0].Value);
            Assert.Equal("two?..", words[^1].Value);
        }
    }
}
