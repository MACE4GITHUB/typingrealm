using System;
using System.Collections.Generic;

namespace TypingRealm.Typing
{
    public interface ITextStore
    {
        IEnumerable<Text> FindAllTexts(Guid userId);
        Text? Find(Guid textId);
        void Append(Guid userId, Text text);
    }
}
