using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface ISentenceRepository
{
    ValueTask SaveAsync(Sentence sentence);
    ValueTask<SentenceId> NextIdAsync();
}
