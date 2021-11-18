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
        public sealed record State(
            Dictionary<int, TypingSessionText> Texts,
            string TypingSessionId,
            DateTime CreatedAtUtc,
            string CreatedByUser,
            TypingSessionConfiguration Configuration) : IIdentifiable
        {
            string IIdentifiable.Id => TypingSessionId;
        }

        private readonly State _state;

        public TypingSession(
            string typingSessionId,
            string createdByUser,
            DateTime createdAtUtc,
            TypingSessionConfiguration configuration)
        {
            // TODO: Validate.

            _state = new State(
                new Dictionary<int, TypingSessionText>(),
                typingSessionId,
                createdAtUtc,
                createdByUser,
                configuration);
        }

        string IIdentifiable.Id => _state.TypingSessionId;

        #region State

        private TypingSession(State state)
        {
            // TODO: Validate.

            _state = state with
            {
                Texts = new Dictionary<int, TypingSessionText>(state.Texts)
            };
        }

        public static TypingSession FromState(State state) => new TypingSession(state);

        public State GetState() => _state with
        {
            Texts = new Dictionary<int, TypingSessionText>(_state.Texts)
        };

        #endregion

        public int AddText(TypingSessionText text)
        {
            var nextIndex = _state.Texts.Count + 1;
            _state.Texts.Add(nextIndex, text);

            return nextIndex;
        }

        public TypingSessionText? GetTypingSessionTextAtIndexOrDefault(int index)
        {
            if (!_state.Texts.ContainsKey(index))
                return null;

            return _state.Texts[index];
        }
    }
}
