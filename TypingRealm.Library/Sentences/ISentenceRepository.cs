using System.Collections.Generic;
using System.Threading.Tasks;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Sentences;

public interface ISentenceRepository
{
    ValueTask SaveByBatchesAsync(
        IEnumerable<Sentence> allSentences,
        int batchSize);

    ValueTask SaveAsync(Sentence sentence);
    ValueTask<SentenceId> NextIdAsync();
    ValueTask RemoveAllForBook(BookId bookId);
}
