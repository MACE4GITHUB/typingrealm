using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library;

public sealed record SentenceDto(
    string SentenceId,
    string Value);

public interface ISentenceQuery
{
    ValueTask<IEnumerable<SentenceDto>> FindRandomSentencesAsync(
        int sentencesCount, int consecutiveSentencesCount = 1);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(
        IEnumerable<string> words, int sentencesCount);

    ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(
        IEnumerable<string> keyPairs, int sentencesCount);
}
