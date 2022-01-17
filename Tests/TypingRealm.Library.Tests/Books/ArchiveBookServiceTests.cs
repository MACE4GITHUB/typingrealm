using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Library.Tests.Books
{
    public class ArchiveBookServiceTests : LibraryTestsBase
    {
        private readonly Mock<ISentenceRepository> _sentenceRepository;
        private readonly Mock<IBookRepository> _bookRepository;
        private readonly ArchiveBookService _sut;

        public ArchiveBookServiceTests()
        {
            _sentenceRepository = Freeze<ISentenceRepository>();
            _bookRepository = Freeze<IBookRepository>();
            _sut = Fixture.Create<ArchiveBookService>();
        }

        [Theory, AutoDomainData]
        public async Task ShouldArchiveBook_ThenUpdateAllSentences_WhenBookExists_AndNotArchived(
            BookId bookId)
        {
            var book = Fixture.CreateBook(config => config
                .With(x => x.IsArchived, false));

            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync(book);

            await _sut.ArchiveBookAsync(bookId);

            _bookRepository.Verify(x => x.UpdateBookAsync(book));
            _bookRepository.Verify(x => x.UpdateBookAsync(It.Is<Book>(book => book.GetState().IsArchived)));
            _sentenceRepository.Verify(x => x.RemoveAllForBook(bookId));
        }

        [Theory, AutoDomainData]
        public async Task ShouldThrow_AndNotUpdateAnything_WhenBookDoesNotExist(
            BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync((Book?)null);

            await AssertThrowsAsync<InvalidOperationException>(
                () => _sut.ArchiveBookAsync(bookId));

            _sentenceRepository.Verify(x => x.RemoveAllForBook(It.IsAny<BookId>()), Times.Never);
            _bookRepository.Verify(x => x.UpdateBookAsync(It.IsAny<Book>()), Times.Never);
        }

        [Theory, AutoDomainData]
        public async Task ShouldNotRemoveSentences_WhenArchivingFails(
            BookId bookId)
        {
            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ThrowsAsync(new TestException());

            await AssertThrowsAsync<TestException>(
                () => _sut.ArchiveBookAsync(bookId));

            _sentenceRepository.Verify(x => x.RemoveAllForBook(bookId), Times.Never);
        }

        [Theory, AutoDomainData]
        public async Task ShouldNotUpdateAnything_WhenBookIsAlreadyArchived(
            BookId bookId)
        {
            var book = Fixture.CreateBook(config => config.With(x => x.IsArchived, true));

            _bookRepository.Setup(x => x.FindBookAsync(bookId))
                .ReturnsAsync(book);

            await AssertThrowsAsync<InvalidOperationException>(
                () => _sut.ArchiveBookAsync(bookId));

            _sentenceRepository.Verify(x => x.RemoveAllForBook(It.IsAny<BookId>()), Times.Never);
            _bookRepository.Verify(x => x.UpdateBookAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public void ShouldBeRegisteredInLibraryDomain()
        {
            var services = new ServiceCollection()
                .AddLibraryDomain()
                .AddInMemoryInfrastructure()
                .BuildServiceProvider();

            services.AssertRegisteredTransient<ArchiveBookService, ArchiveBookService>();
        }
    }
}
