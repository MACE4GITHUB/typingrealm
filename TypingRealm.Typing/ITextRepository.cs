using System.Threading.Tasks;

namespace TypingRealm.Typing;

public interface ITextRepository
{
    ValueTask<Text?> FindAsync(string textId);
    ValueTask SaveAsync(Text text);
    ValueTask<string> NextIdAsync();
}
