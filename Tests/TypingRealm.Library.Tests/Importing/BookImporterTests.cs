using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Importing;
using TypingRealm.Library.Sentences;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Library.Tests.Importing;

public class BookImporterTests : LibraryTestsBase
{
    private readonly Mock<IBookRepository> _bookRepository;
    private readonly Mock<ISentenceRepository> _sentenceRepository;
    private readonly Mock<IBookContentProcessor> _bookContentProcessor;
    private readonly BookImporter _sut;

    public BookImporterTests()
    {
        _bookRepository = Freeze<IBookRepository>();
        _sentenceRepository = Freeze<ISentenceRepository>();
        _bookContentProcessor = Freeze<IBookContentProcessor>();
        _sut = Create<BookImporter>();
    }

    [Theory, AutoDomainData]
    public async Task ShouldThrow_WhenBookIsNull()
    {
        await AssertThrowsAsync<ArgumentNullException>(
            async () => await _sut.ImportBookAsync(null!));
    }

    [Theory, AutoDomainData]
    public async Task ShouldThrow_WhenBookContentNotFound(Book book)
    {
        _bookRepository.Setup(x => x.FindBookContentAsync(book.BookId))
            .ReturnsAsync((BookContent?)null);

        await AssertThrowsAsync<InvalidOperationException>(
            async () => await _sut.ImportBookAsync(book));
    }

    [Theory]
    [InlineDomainData(ProcessingStatus.NotProcessed)]
    [InlineDomainData(ProcessingStatus.Processed)]
    [InlineDomainData(ProcessingStatus.Error)]
    [InlineDomainData(ProcessingStatus.Processing)]
    public async Task ShouldUpdateBookStatus_ToProcessing_FromAnyStatus_EvenProcessing(
        ProcessingStatus processingStatus, Book book)
    {
        _bookRepository.Setup(x => x.FindBookAsync(book.BookId))
            .ReturnsAsync(Fixture.CreateBook(config =>
                config.With(book => book.IsArchived, false)
                    .With(book => book.ProcessingStatus, processingStatus)));

        var updates = new List<Book.State>();
        _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
            .Callback<Book>(book => updates.Add(book.GetState()));

        await _sut.ImportBookAsync(book);

        Assert.Equal(ProcessingStatus.Processing, updates[0].ProcessingStatus);
    }

    [Theory]
    [InlineDomainData(ProcessingStatus.NotProcessed)]
    [InlineDomainData(ProcessingStatus.Processed)]
    [InlineDomainData(ProcessingStatus.Error)]
    [InlineDomainData(ProcessingStatus.Processing)]
    public async Task ShouldRemoveAllSentences_BeforeImportingABook(
        ProcessingStatus processingStatus, Book book)
    {
        _bookRepository.Setup(x => x.FindBookAsync(book.BookId))
            .ReturnsAsync(Fixture.CreateBook(config =>
                config.With(book => book.IsArchived, false)
                    .With(book => book.ProcessingStatus, processingStatus)));

        var updates = new List<Book.State?>();
        _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
            .Callback<Book>(book => updates.Add(book.GetState()));
        _sentenceRepository.Setup(x => x.RemoveAllForBook(book.BookId))
            .Callback(() => updates.Add(null));


        await _sut.ImportBookAsync(book);

        Assert.Equal(ProcessingStatus.Processing, updates[0]?.ProcessingStatus);
        Assert.Null(updates[1]);
    }

    [Theory]
    [InlineDomainData(ProcessingStatus.NotProcessed)]
    [InlineDomainData(ProcessingStatus.Processed)]
    [InlineDomainData(ProcessingStatus.Error)]
    [InlineDomainData(ProcessingStatus.Processing)]
    public async Task ShouldNotAllowImport_WhenBookIsArchived(
        ProcessingStatus processingStatus)
    {
        var book = Fixture.CreateBook(config =>
            config.With(book => book.IsArchived, true)
                .With(book => book.ProcessingStatus, processingStatus));

        _bookRepository.Setup(x => x.FindBookAsync(book.BookId))
            .ReturnsAsync(book);

        var updates = new List<Book.State?>();
        _bookRepository.Setup(x => x.UpdateBookAsync(It.IsAny<Book>()))
            .Callback<Book>(book => updates.Add(book.GetState()));
        _sentenceRepository.Setup(x => x.RemoveAllForBook(book.BookId))
            .Callback(() => updates.Add(null));

        await AssertThrowsAsync<DomainException>(
            async () => await _sut.ImportBookAsync(book));

        Assert.Empty(updates);
    }

    [Fact]
    public void AddLibraryDomain_ShouldRegisterTransient()
    {
        var serviceProvider = GetServiceCollection()
            .AddLibraryDomain()
            .BuildServiceProvider();

        serviceProvider.AssertRegisteredTransient<IBookImporter, BookImporter>();
    }
}
