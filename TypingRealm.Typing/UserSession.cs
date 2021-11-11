using System;
using System.Collections.Generic;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Aggregate root.
    /// </summary>
    public sealed class UserSession : IIdentifiable
    {
        private readonly string _userSessionId;
        private readonly string _userId;
        private readonly string _typingSessionId;
        private readonly DateTime _createdUtc;
        private readonly TimeZoneInfo _userTimeZone;
        private readonly List<TextTypingResult> _textTypingResults = new List<TextTypingResult>();

        public UserSession(
            string userSessionId,
            string userId,
            string typingSessionId,
            DateTime createdUtc,
            TimeZoneInfo userTimeZone)
        {
            _userSessionId = userSessionId;
            _userId = userId;
            _typingSessionId = typingSessionId;
            _createdUtc = createdUtc;
            _userTimeZone = userTimeZone;
        }

        public string Id => _userSessionId;
        public string TypingSessionId => _typingSessionId;

        public void LogResult(TextTypingResult textTypingResult)
        {
            // TODO: Validate. (results should follow each other with max allowed pause, etc)
            // Consider allowing logging different results from different texts: like you can type 6th text
            // in the beginning, and then type 2nd text two times, everything will be logged.

            _textTypingResults.Add(textTypingResult);
        }
    }
}
