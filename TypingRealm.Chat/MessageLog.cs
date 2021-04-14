using System.Collections.Generic;
using TypingRealm.Messaging;

namespace TypingRealm.Chat
{
    [Message]
    public sealed class MessageLog
    {
        public MessageLog() { }

        public List<string> Messages { get; set; }
            = new List<string>();
    }
}
