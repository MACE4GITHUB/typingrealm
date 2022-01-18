using System;
using AutoFixture;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;
using Xunit;

namespace TypingRealm.Library.Tests.Books
{
    public class BookTests : LibraryTestsBase
    {
        [Theory, AutoDomainData]
        public void ShouldCreate_FromState(Book.State state)
        {
            var sut = Book.FromState(state);

            Assert.Equal(sut.GetState(), state);
        }

        [Theory, AutoDomainData]
        public void ShouldCreate(
            BookId bookId, Language language, BookDescription description)
        {
            var sut = new Book(bookId, language, description);

            var state = sut.GetState();
            Assert.Equal(bookId, state.BookId);
            Assert.Equal(language, state.Language);
            Assert.Equal(description, state.Description);
        }

        [Fact]
        public void ShouldNotBeProcessed_AfterCreation()
        {
            var sut = CreateNewBook();

            var state = sut.GetState();
            Assert.Equal(ProcessingStatus.NotProcessed, state.ProcessingStatus);
        }

        [Fact]
        public void ShouldNotBeArchived_AfterCreation()
        {
            var sut = CreateNewBook();

            var state = sut.GetState();
            Assert.False(state.IsArchived);
        }

        [Theory, AutoDomainData]
        public void ShouldThrow_WhenBookIdIsNull(
            BookId bookId, Language language, BookDescription description)
        {
            Assert.Throws<ArgumentNullException>(() => new Book(null!, language, description));
            Assert.Throws<ArgumentNullException>(() => new Book(bookId, null!, description));
            Assert.Throws<ArgumentNullException>(() => new Book(bookId, language, null!));
        }

        [Theory, AutoDomainData]
        public void ShouldExposeBookId(Book sut)
        {
            var state = sut.GetState();

            Assert.Equal(state.BookId, sut.BookId);
        }

        [Theory, AutoDomainData]
        public void ShouldExposeLanguage(Book sut)
        {
            var state = sut.GetState();

            Assert.Equal(state.Language, sut.Language);
        }

        [Theory, AutoDomainData]
        public void Describe_ShouldUpdateDescription(BookDescription description)
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false));

            Assert.NotEqual(description, sut.GetState().Description);

            sut.Describe(description);
            Assert.Equal(description, sut.GetState().Description);
        }

        [Theory, AutoDomainData]
        public void Describe_ShouldThrow_WhenDescriptionIsNull(Book sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.Describe(null!));
        }

        [Theory, AutoDomainData]
        public void Describe_ShouldWork_EvenOnArchivedBooks(BookDescription description)
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true));

            Assert.NotEqual(description, sut.GetState().Description);

            sut.Describe(description);
            Assert.Equal(description, sut.GetState().Description);
        }

        [Fact]
        public void StartProcessing_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true));

            Assert.Throws<InvalidOperationException>(() => sut.StartProcessing());
        }

        [Theory]
        [InlineData(ProcessingStatus.NotProcessed)]
        [InlineData(ProcessingStatus.Processed)]
        [InlineData(ProcessingStatus.Processing)]
        [InlineData(ProcessingStatus.Error)]
        public void StartProcessing_ShouldSetToProcessing(ProcessingStatus status)
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.ProcessingStatus, status));

            sut.StartProcessing();

            Assert.Equal(ProcessingStatus.Processing, sut.GetState().ProcessingStatus);
        }

        [Fact]
        public void FinishProcessing_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true)
                .With(x => x.ProcessingStatus, ProcessingStatus.Processing));

            Assert.Throws<InvalidOperationException>(() => sut.FinishProcessing());
        }

        [Theory]
        [InlineData(ProcessingStatus.NotProcessed)]
        [InlineData(ProcessingStatus.Processed)]
        [InlineData(ProcessingStatus.Error)]
        public void FinishProcessing_ShouldThrow_WhenNotProcessing(ProcessingStatus status)
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.ProcessingStatus, status));

            Assert.Throws<InvalidOperationException>(() => sut.FinishProcessing());
        }

        [Fact]
        public void FinishProcessing_ShouldMarkAsProcessed()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.ProcessingStatus, ProcessingStatus.Processing));

            sut.FinishProcessing();

            Assert.Equal(ProcessingStatus.Processed, sut.GetState().ProcessingStatus);
        }

        [Fact]
        public void ErrorProcessing_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true)
                .With(x => x.ProcessingStatus, ProcessingStatus.Processing));

            Assert.Throws<InvalidOperationException>(() => sut.ErrorProcessing());
        }

        [Theory]
        [InlineData(ProcessingStatus.NotProcessed)]
        [InlineData(ProcessingStatus.Processed)]
        [InlineData(ProcessingStatus.Error)]
        public void ErrorProcessing_ShouldThrow_WhenNotProcessing(ProcessingStatus status)
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.ProcessingStatus, status));

            Assert.Throws<InvalidOperationException>(() => sut.ErrorProcessing());
        }

        [Fact]
        public void ErrorProcessing_ShouldMarkAsErrored()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.ProcessingStatus, ProcessingStatus.Processing));

            sut.ErrorProcessing();

            Assert.Equal(ProcessingStatus.Error, sut.GetState().ProcessingStatus);
        }

        [Fact]
        public void Archive_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true));

            Assert.Throws<InvalidOperationException>(() => sut.Archive());
        }

        [Fact]
        public void Archive_ShouldArchiveBook()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false));

            sut.Archive();

            Assert.True(sut.GetState().IsArchived);
        }

        // Primitives.

        [Fact]
        public void BookDescription_ShouldBeBetween1And100Characters()
        {
            Assert.Throws<ArgumentNullException>(() => new BookDescription(null!));
            Assert.Throws<ArgumentException>(() => new BookDescription(string.Empty));
            Assert.Throws<ArgumentException>(() => new BookDescription(new string('a', 101)));

            _ = new BookDescription(new string('a', 100));
            _ = new BookDescription("a");
        }

        [Fact]
        public void BookId_ShouldBeBetween1And50Characters()
        {
            Assert.Throws<ArgumentNullException>(() => new BookId(null!));
            Assert.Throws<ArgumentException>(() => new BookId(string.Empty));
            Assert.Throws<ArgumentException>(() => new BookId(new string('a', 51)));

            _ = new BookId(new string('a', 50));
            _ = new BookId("a");
        }

        [Fact]
        public void BookId_New_ShouldCreateNewGuid()
        {
            Assert.True(Guid.TryParse(BookId.New(), out var id));
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void SentenceId_ShouldBeBetween1And50Characters()
        {
            Assert.Throws<ArgumentNullException>(() => new SentenceId(null!));
            Assert.Throws<ArgumentException>(() => new SentenceId(string.Empty));
            Assert.Throws<ArgumentException>(() => new SentenceId(new string('a', 51)));

            _ = new SentenceId(new string('a', 50));
            _ = new SentenceId("a");
        }

        [Fact]
        public void SentenceId_New_ShouldCreateNewGuid()
        {
            Assert.True(Guid.TryParse(SentenceId.New(), out var id));
            Assert.NotEqual(Guid.Empty, id);
        }

        private Book CreateNewBook()
        {
            return new Book(
                Fixture.Create<BookId>(),
                Fixture.Create<Language>(),
                Fixture.Create<BookDescription>());
        }
    }
}
