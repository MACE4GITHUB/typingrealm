using System.Collections.Generic;
using System.Linq;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Importing;

public interface IBookContentProcessor
{
    IEnumerable<Sentence> ProcessBookContent(
        BookId bookId, string content, LanguageInformation languageInformation);
}

public sealed class BookContentProcessor : IBookContentProcessor
{
    private readonly SentenceValidator _sentenceValidator = new SentenceValidator();
    private readonly ITextProcessor _textProcessor;
    private readonly ISentenceFactory _sentenceFactory;

    public BookContentProcessor(
        ITextProcessor textProcessor,
        ISentenceFactory sentenceFactory)
    {
        _textProcessor = textProcessor;
        _sentenceFactory = sentenceFactory;
    }

    public IEnumerable<Sentence> ProcessBookContent(BookId bookId, string content, LanguageInformation languageInformation)
    {
        return _textProcessor.GetSentencesEnumerable(content, languageInformation)
            .Where(sentence => _sentenceValidator.IsValidSentence(sentence))
            .Select((sentence, sentenceIndex) => _sentenceFactory.CreateSentence(bookId, sentence, sentenceIndex));
    }
}
