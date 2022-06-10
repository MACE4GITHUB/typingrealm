using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.InMemoryInfrastructure;

public sealed class InMemorySentenceRepository : ISentenceRepository, ISentenceQuery
{
    private readonly Dictionary<SentenceId, Sentence> _sentences
        = new Dictionary<SentenceId, Sentence>();

    public async ValueTask<IEnumerable<SentenceDto>> FindSentencesAsync(SentencesRequest request)
    {
        return _sentences.Values.Select(x => new SentenceDto(x.SentenceId, x.Value));
        /*{
            SentenceId = x.SentenceId,
            Value = x.Value
        });*/
    }

    public async ValueTask<IEnumerable<string>> FindWordsAsync(WordsRequest request)
    {
        return (await FindSentencesAsync(null!).ConfigureAwait(false)).SelectMany(x => x.Value)!.ToString()!
            .Split(' ');
    }

    public ValueTask<SentenceId> NextIdAsync()
    {
        return new(SentenceId.New());
    }

    public ValueTask RemoveAllForBook(BookId bookId)
    {
        foreach (var sentence in _sentences.Where(sentence => sentence.Value.BookId == bookId).ToList())
        {
            _sentences.Remove(sentence.Key);
        }

        return default;
    }

    public ValueTask SaveAsync(Sentence sentence)
    {
        if (!_sentences.ContainsKey(sentence.SentenceId))
            _sentences.Add(sentence.SentenceId, sentence);
        else
            _sentences[sentence.SentenceId] = sentence;

        return default;
    }

    public async ValueTask SaveByBatchesAsync(IEnumerable<Sentence> allSentences)
    {
        foreach (var sentence in allSentences)
        {
            await SaveAsync(sentence)
                .ConfigureAwait(false);
        }
    }
}
