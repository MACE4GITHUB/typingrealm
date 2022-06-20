using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Infrastructure;
using TypingRealm.Library.Sentences;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Library.Tests.Books;

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
    public async Task ShouldArchiveBook_ThenRemoveAllSentences_WhenNotArchived()
    {
        var book = Fixture.CreateBook(config => config
            .With(x => x.IsArchived, false));

        await _sut.ArchiveBookAsync(book);

        _bookRepository.Verify(x => x.UpdateBookAsync(book));
        _bookRepository.Verify(x => x.UpdateBookAsync(It.Is<Book>(book => book.GetState().IsArchived)));
        _sentenceRepository.Verify(x => x.RemoveAllForBook(book.BookId));
    }

    [Theory, AutoDomainData]
    public async Task ShouldThrow_AndNotUpdateAnything_WhenBookIsNull()
    {
        await AssertThrowsAsync<ArgumentNullException>(
            () => _sut.ArchiveBookAsync(null!));

        _sentenceRepository.Verify(x => x.RemoveAllForBook(It.IsAny<BookId>()), Times.Never);
        _bookRepository.Verify(x => x.UpdateBookAsync(It.IsAny<Book>()), Times.Never);
    }

    [Theory, AutoDomainData]
    public async Task ShouldNotRemoveSentences_WhenArchivingFails(
        Book book)
    {
        _bookRepository.Setup(x => x.UpdateBookAsync(book))
            .ThrowsAsync(NewTestException());

        await AssertThrowsAsync<TestException>(
            () => _sut.ArchiveBookAsync(book));

        _sentenceRepository.Verify(x => x.RemoveAllForBook(book.BookId), Times.Never);
    }

    [Theory, AutoDomainData]
    public async Task ShouldNotUpdateAnything_WhenBookIsAlreadyArchived()
    {
        var book = Fixture.CreateBook(config => config.With(x => x.IsArchived, true));

        await AssertThrowsAsync<DomainException>(
            () => _sut.ArchiveBookAsync(book));

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
