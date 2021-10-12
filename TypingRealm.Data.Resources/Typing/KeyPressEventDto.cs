using System;
using System.Collections.Generic;

namespace TypingRealm.Data.Resources.Typing
{
#pragma warning disable CS8618
    public sealed class KeyPressEventDto
    {
        public string Key { get; set; }
        public decimal Perf { get; set; }
        public int Index { get; set; }
    }

    public sealed class TypedTextDto
    {
        public TextData TextData { get; set; }
        public DateTimeOffset StartedTypingAt { get; set; }
        public IEnumerable<KeyPressEventDto> Events { get; set; }
    }

    public sealed class TextData
    {
        public string Text { get; set; }
    }
#pragma warning restore CS8618
}
