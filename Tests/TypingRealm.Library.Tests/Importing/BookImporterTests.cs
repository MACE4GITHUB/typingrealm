using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Importing;
using TypingRealm.Library.Sentences;
using Xunit;

namespace TypingRealm.Library.Tests.Importing
{
    public class BookImporterTests : LibraryTestsBase
    {
        private readonly Mock<IBookRepository> _bookRepository;
        private readonly Mock<ISentenceRepository> _sentenceRepository;
        private readonly BookImporter _sut;

        public BookImporterTests()
        {
            _bookRepository = Freeze<IBookRepository>();
            _sentenceRepository = Freeze<ISentenceRepository>();
            _sut = Create<BookImporter>();
        }

        [Theory, AutoDomainData]
        public async Task ShouldThrow_WhenBookNotFound(BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync((Book?)null);

            await AssertThrowsAsync<InvalidOperationException>(
                async () => await _sut.ImportBookAsync(bookId));
        }

        [Theory, AutoDomainData]
        public async Task ShouldThrow_WhenBookContentNotFound(BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookContentAsync(bookId))
                .ReturnsAsync((BookContent?)null);

            await AssertThrowsAsync<InvalidOperationException>(
                async () => await _sut.ImportBookAsync(bookId));
        }

        [Theory]
        [InlineDomainData(ProcessingStatus.NotProcessed)]
        [InlineDomainData(ProcessingStatus.Processed)]
        [InlineDomainData(ProcessingStatus.Error)]
        [InlineDomainData(ProcessingStatus.Processing)]
        public async Task ShouldUpdateBookStatus_ToProcessing_FromAnyStatus_EvenProcessing(
            ProcessingStatus processingStatus, BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync(Fixture.CreateBook(config =>
                    config.With(book => book.IsArchived, false)
                        .With(book => book.ProcessingStatus, processingStatus)));

            var updates = new List<Book.State>();
            _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
                .Callback<Book>(book => updates.Add(book.GetState()));

            await _sut.ImportBookAsync(bookId);

            Assert.Equal(ProcessingStatus.Processing, updates[0].ProcessingStatus);
        }

        [Theory]
        [InlineDomainData(ProcessingStatus.NotProcessed)]
        [InlineDomainData(ProcessingStatus.Processed)]
        [InlineDomainData(ProcessingStatus.Error)]
        [InlineDomainData(ProcessingStatus.Processing)]
        public async Task ShouldRemoveAllSentences_BeforeImportingABook(
            ProcessingStatus processingStatus, BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync(Fixture.CreateBook(config =>
                    config.With(book => book.IsArchived, false)
                        .With(book => book.ProcessingStatus, processingStatus)));

            var updates = new List<Book.State?>();
            _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
                .Callback<Book>(book => updates.Add(book.GetState()));
            _sentenceRepository.Setup(x => x.RemoveAllForBook(bookId))
                .Callback(() => updates.Add(null));


            await _sut.ImportBookAsync(bookId);

            Assert.Equal(ProcessingStatus.Processing, updates[0]?.ProcessingStatus);
            Assert.Null(updates[1]);
        }

        [Theory]
        [InlineDomainData(ProcessingStatus.NotProcessed)]
        [InlineDomainData(ProcessingStatus.Processed)]
        [InlineDomainData(ProcessingStatus.Error)]
        [InlineDomainData(ProcessingStatus.Processing)]
        public async Task ShouldNotAllowImport_WhenBookIsArchived(
            ProcessingStatus processingStatus, BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync(Fixture.CreateBook(config =>
                    config.With(book => book.IsArchived, true)
                        .With(book => book.ProcessingStatus, processingStatus)));

            var updates = new List<Book.State?>();
            _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
                .Callback<Book>(book => updates.Add(book.GetState()));
            _sentenceRepository.Setup(x => x.RemoveAllForBook(bookId))
                .Callback(() => updates.Add(null));

            await AssertThrowsAsync<InvalidOperationException>(
                async () => await _sut.ImportBookAsync(bookId));

            Assert.Empty(updates);
        }
    }
}
