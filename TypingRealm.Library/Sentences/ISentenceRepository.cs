using System.Collections.Generic;
using System.Threading.Tasks;
using TypingRealm.Library.Books;

namespace TypingRealm.Library.Sentences;

public interface ISentenceRepository
{
    ValueTask<SentenceId> NextIdAsync();
    ValueTask SaveByBatchesAsync(IEnumerable<Sentence> allSentences);
    ValueTask SaveAsync(Sentence sentence);
    ValueTask RemoveAllForBook(BookId bookId);
}
