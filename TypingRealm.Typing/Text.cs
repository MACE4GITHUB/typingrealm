using System;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Aggregate root. User-defined text or randomly generated one. We can reuse
    /// the same text only if we are sure it will never change. Either we don't
    /// allow to change the value of the text, or we need to generate new texts
    /// every time.
    /// </summary>
    public sealed class Text : IIdentifiable
    {
        public sealed record State(
            string TextId,
            string Value,
            string CreatedByUser,
            DateTime CreatedUtc,
            bool IsPublic) : IIdentifiable
        {
            string IIdentifiable.Id => TextId;
        }

        private State _state;

        public Text(string textId, string value, string createdByUser, DateTime createdUtc, bool isPublic)
        {
            // TODO: Validate.

            _state = new State(textId, value, createdByUser, createdUtc, isPublic);
        }

        string IIdentifiable.Id => _state.TextId;

        #region State

        private Text(State state)
        {
            // TODO: Validate.

            _state = state with { };
        }

        public static Text FromState(State state) => new Text(state);

        public State GetState() => _state with { };

        #endregion

        public string Value => _state.Value;

        public void MakePublic()
        {
            if (_state.IsPublic)
                throw new InvalidOperationException("Text is already public.");

            _state = _state with { IsPublic = true };
        }

        public void MakePrivate()
        {
            if (!_state.IsPublic)
                throw new InvalidOperationException("Test is already private.");

            _state = _state with { IsPublic = false };
        }
    }
}
