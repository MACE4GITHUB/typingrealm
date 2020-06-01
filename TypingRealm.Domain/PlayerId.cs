using TypingRealm.Domain.Common;

namespace TypingRealm.Domain
{
    public sealed class PlayerId : Identity<string>
    {
        public PlayerId(string value) : base(value)
        {
        }
    }
}
