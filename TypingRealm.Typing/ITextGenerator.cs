using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface ITextGenerator
    {
        /// <summary>
        /// Generates text based on configuration.
        /// </summary>
        ValueTask<string> GenerateTextAsync(TextConfiguration configuration);
    }
}
