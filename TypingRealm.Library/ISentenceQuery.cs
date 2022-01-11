using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library;

public delegate ISentenceQuery SentenceQueryResolver(string language);
public interface ISentenceQuery
{
    ValueTask<IEnumerable<SentenceDto>> FindRandomSentencesAsync(
        int maxSentencesCount, int consecutiveSentencesCount);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(
        IEnumerable<string> words, int maxSentencesCount);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(
        IEnumerable<string> keyPairs, int maxSentencesCount);

    ValueTask<IEnumerable<string>> FindWordsContainingKeyPairsAsync(
        IEnumerable<string> keyPairs, int maxWordsCount, bool rawWords);
}
