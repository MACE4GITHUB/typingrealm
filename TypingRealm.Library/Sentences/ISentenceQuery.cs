using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library.Sentences;

public delegate ISentenceQuery SentenceQueryResolver(string language);
public interface ISentenceQuery
{
    ValueTask<IEnumerable<SentenceDto>> FindSentencesAsync(SentencesRequest request);
    ValueTask<IEnumerable<string>> FindWordsAsync(WordsRequest request);
}
