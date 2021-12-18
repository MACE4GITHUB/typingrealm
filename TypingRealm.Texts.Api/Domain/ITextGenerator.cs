using System.Threading.Tasks;

namespace TypingRealm.Texts
{
    public interface ITextGenerator
    {
        string Language { get; }
        ValueTask<string> GenerateTextAsync(TextGenerationConfiguration configuration);
    }
}
