using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library;

public interface ISentenceQuery
{
    ValueTask<IEnumerable<SentenceDto>> FindRandomSentencesAsync(
        int sentencesCount, int consecutiveSentencesCount);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(
        IEnumerable<string> words, int maxSentencesCount);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(
        IEnumerable<string> keyPairs, int maxSentencesCount);
}
