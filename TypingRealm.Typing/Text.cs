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
        private readonly string _textId;
        private readonly string _value;
        private readonly string _createdByUser;
        private readonly DateTime _createdUtc;
        private bool _isPublic;

        public Text(string textId, string value, string createdByUser, DateTime createdUtc, bool isPublic)
        {
            _textId = textId;
            _value = value;
            _createdByUser = createdByUser;
            _createdUtc = createdUtc;
            _isPublic = isPublic;
        }

        public string Id => _textId;
        public string Value => _value;

        public void MakePublic()
        {
            _isPublic = true;
        }

        public void MakePrivate()
        {
            _isPublic = false;
        }
    }
}
