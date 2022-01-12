using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface ISentenceRepository
{
    ValueTask SaveByBatchesAsync(
        IEnumerable<Sentence> allSentences,
        int batchSize);

    ValueTask SaveAsync(Sentence sentence);
    ValueTask<SentenceId> NextIdAsync();
    ValueTask RemoveAllForBook(BookId bookId);
}
