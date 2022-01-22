using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Importing;

public interface IBookImporter
{
    ValueTask<BookImportResult> ImportBookAsync(BookId bookId);
}

public sealed class BookImporter : IBookImporter
{
    private const int MinSentenceLengthCharacters = 8;
    private readonly IBookRepository _bookStore;
    private readonly ISentenceRepository _sentenceRepository;
    private readonly ITextProcessor _textProcessor;
    private readonly ILanguageProvider _languageProvider;
    private readonly IBookContentProcessor _bookContentProcessor;
    private readonly ILogger<BookImporter> _logger;

    public BookImporter(
        IBookRepository bookStore,
        ISentenceRepository sentenceRepository,
        ITextProcessor textProcessor,
        ILanguageProvider languageProvider,
        IBookContentProcessor bookContentProcessor,
        ILogger<BookImporter> logger)
    {
        _bookStore = bookStore;
        _sentenceRepository = sentenceRepository;
        _textProcessor = textProcessor;
        _languageProvider = languageProvider;
        _bookContentProcessor = bookContentProcessor;
        _logger = logger;
    }

    public async ValueTask<BookImportResult> ImportBookAsync(BookId bookId)
    {
        var book = await _bookStore.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book {bookId} is not found.");

        var bookContent = await _bookStore.FindBookContentAsync(bookId)
            .ConfigureAwait(false);

        if (bookContent == null)
            throw new InvalidOperationException($"Content for the book {bookId} is not found.");

        book.StartProcessing();

        await _bookStore.UpdateBookAsync(book)
            .ConfigureAwait(false);

        await _sentenceRepository.RemoveAllForBook(bookId)
            .ConfigureAwait(false);

        // TODO: Everything below is not unit tested yet.
        try
        {
            var result = await ImportBookAsync(book, bookContent)
                .ConfigureAwait(false);

            book.FinishProcessing();

            await _bookStore.UpdateBookAsync(book)
                .ConfigureAwait(false);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while importing the book {BookId}.", bookId.Value);
            book.ErrorProcessing();

            await _bookStore.UpdateBookAsync(book)
                .ConfigureAwait(false);

            throw new InvalidOperationException($"Error while importing book {bookId}");
        }
    }

    private async ValueTask<BookImportResult> ImportBookAsync(Book book, BookContent bookContent)
    {
        using var reader = new StreamReader(bookContent.Content);
        var content = await reader.ReadToEndAsync()
            .ConfigureAwait(false);

        var languageInfo = await _languageProvider.FindLanguageInformationAsync(book.Language)
            .ConfigureAwait(false);

        var tooShortSentences = _textProcessor.GetSentencesEnumerable(content, languageInfo)
            .Where(sentence => sentence.Length < MinSentenceLengthCharacters)
            .Distinct()
            .ToList();

        var notAllowedSentences = _textProcessor.GetSentencesEnumerable(content)
            .Where(sentence => !languageInfo.IsAllLettersAllowed(sentence))
            .Distinct()
            .ToList();

        var notAllowedCharacters = notAllowedSentences.SelectMany(
            sentence => sentence.Where(character => !languageInfo.IsAllLettersAllowed(character.ToString())))
            .Distinct()
            .ToList();

        var sentencesEnumerable = _bookContentProcessor.ProcessBookContent(
            book.BookId, content, languageInfo);

        await _sentenceRepository.SaveByBatchesAsync(sentencesEnumerable)
            .ConfigureAwait(false);

        return new BookImportResult(
            book,
            tooShortSentences,
            notAllowedSentences,
            string.Join(string.Empty, notAllowedCharacters));
    }
}
