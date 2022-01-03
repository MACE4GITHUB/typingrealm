using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TypingRealm.Texts.Retrievers;

public sealed class OfflineTextRetriever : ITextRetriever
{
    private readonly List<string> _texts;

    public OfflineTextRetriever(string language, IEnumerable<string> texts)
    {
        _texts = texts.ToList();
        Language = language;
    }

    public string Language { get; }

    public ValueTask<string> RetrieveTextAsync()
    {
        var index = RandomNumberGenerator.GetInt32(0, _texts.Count);

        return new(_texts[index]);
    }
}
