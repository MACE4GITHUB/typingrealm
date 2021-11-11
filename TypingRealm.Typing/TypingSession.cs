using System;
using System.Collections.Generic;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Aggregate root.
    ///
    /// Typing session can have multiple TypingSessionTexts when it's a
    /// continuous session where you type multiple texts. Every text should have
    /// its own index and they all should start from 1 and go consecutively in
    /// order.
    ///
    /// For one TypingSession there could be multiple users typing the same set
    /// of texts - UserSessions.
    /// </summary>
    public sealed class TypingSession : IIdentifiable
    {
        private readonly Dictionary<int, TypingSessionText> _texts = new Dictionary<int, TypingSessionText>();
        private readonly string _typingSessionId;
        private readonly DateTime _createdAtUtc;
        private readonly string _createdByUser;
        private readonly TypingSessionConfiguration _configuration;

        public TypingSession(
            string typingSessionId,
            string createdByUser,
            DateTime createdAtUtc,
            TypingSessionConfiguration configuration)
        {
            _typingSessionId = typingSessionId;
            _createdAtUtc = createdAtUtc;
            _createdByUser = createdByUser;
            _configuration = configuration;
        }

        public string Id => _typingSessionId;

        public int AddText(TypingSessionText text)
        {
            var nextIndex = _texts.Count + 1;
            _texts.Add(nextIndex, text);

            return nextIndex;
        }

        public TypingSessionText? GetTypingSessionTextAtIndexOrDefault(int index)
        {
            if (!_texts.ContainsKey(index))
                return null;

            return _texts[index];
        }
    }
}
