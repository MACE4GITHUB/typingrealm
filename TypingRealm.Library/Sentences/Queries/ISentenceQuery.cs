using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Library.Sentences.Queries;

public delegate ISentenceQuery SentenceQueryResolver(string language);
public interface ISentenceQuery
{
    ValueTask<IEnumerable<SentenceView>> FindSentencesAsync(SentencesRequest request);
    ValueTask<IEnumerable<string>> FindWordsAsync(WordsRequest request);
}
