using System.Collections.Generic;

namespace TypingRealm.Library
{
    public interface ISentenceQuery
    {
        IEnumerable<string> FindSentencesContainingWordsAsync(IEnumerable<string> words, int count);
        IEnumerable<string> FindSentencesContainingKeyPairsAsync(IEnumerable<string> keyPairs, int count);
    }
}
