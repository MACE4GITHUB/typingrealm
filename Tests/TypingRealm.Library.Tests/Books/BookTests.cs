﻿using System;
using AutoFixture;
using TypingRealm.Library.Books;
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
            Assert.False(state.IsProcessed);
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
        public void ShouldExposeIsProcessed(Book sut)
        {
            var state = sut.GetState();

            Assert.Equal(state.IsProcessed, sut.IsProcessed);
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
        public void StartReprocessing_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true));

            Assert.Throws<InvalidOperationException>(() => sut.StartReprocessing());
        }

        [Fact]
        public void StartReprocessing_ShouldSetProcessedToFalse()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.IsProcessed, false));

            sut.StartReprocessing();

            Assert.False(sut.GetState().IsProcessed);

            sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.IsProcessed, true));

            sut.StartReprocessing();

            Assert.False(sut.GetState().IsProcessed);
        }

        [Fact]
        public void FinishProcessing_ShouldThrow_WhenAlreadyArchived()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, true)
                .With(x => x.IsProcessed, false));

            Assert.Throws<InvalidOperationException>(() => sut.FinishProcessing());
        }

        [Fact]
        public void FinishProcessing_ShouldThrow_WhenAlreadyProcessed()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.IsProcessed, true));

            Assert.Throws<InvalidOperationException>(() => sut.FinishProcessing());
        }

        [Fact]
        public void FinishProcessing_ShouldMarkAsProcessed()
        {
            var sut = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false)
                .With(x => x.IsProcessed, false));

            sut.FinishProcessing();

            Assert.True(sut.GetState().IsProcessed);
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

        private Book CreateNewBook()
        {
            return new Book(
                Fixture.Create<BookId>(),
                Fixture.Create<Language>(),
                Fixture.Create<BookDescription>());
        }
    }
}
