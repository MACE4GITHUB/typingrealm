using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public sealed class InMemoryTextStore : ITextStore
    {
        private readonly Dictionary<Guid, List<Text>> _userTexts
             = new Dictionary<Guid, List<Text>>();

        public void Append(Guid userId, Text text)
        {
            if (!_userTexts.ContainsKey(userId))
                _userTexts.Add(userId, new List<Text>());

            var userTexts = _userTexts[userId];
            userTexts.Add(text);
        }

        public Text? Find(Guid textId)
        {
            foreach (var users in _userTexts.Values)
            {
                var text = users.FirstOrDefault(x => x.TextId == textId);
                if (text != null)
                    return text;
            }

            return null;
        }

        public IEnumerable<Text> FindAllTexts(Guid userId)
        {
            if (!_userTexts.ContainsKey(userId))
                _userTexts.Add(userId, new List<Text>());

            return _userTexts[userId];
        }
    }
}
