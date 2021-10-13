using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface ITextGenerator
    {
        ValueTask<string> GetTextAsync();
    }
}
