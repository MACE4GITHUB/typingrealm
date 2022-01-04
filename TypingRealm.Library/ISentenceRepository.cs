using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface ISentenceRepository
{
    ValueTask SaveBulkAsync(IEnumerable<Sentence> sentences);
    ValueTask SaveAsync(Sentence sentence);
    ValueTask<SentenceId> NextIdAsync();
}
