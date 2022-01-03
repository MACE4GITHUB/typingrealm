using System.Text.Json.Serialization;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Entity of TextTypingResult, which is an entity of UserSession.
    ///
    /// Identifies single key press at any given time, with the delay that
    /// occurred before pressing the key after the previous key (be it a key or
    /// backspace). The first pressed key in the text has zero delay, no other
    /// key presses should have delay of zero.
    /// </summary>
    /// <param name="Index">Zero-based index of the key in the text. It will be
    /// zero for the first character, all consecutive key press events should
    /// have consecutive increasing indexes without skips.</param>
    /// <param name="Key">Key that was pressed. "a" for "a", "backspace" for
    /// backspace key.</param>
    /// <param name="Delay">Delay between the previous key and before pressing
    /// this one, in milliseconds with decimal point (micro-second precision).</param>
    public sealed record KeyPressEvent(
        int Index,
        KeyAction KeyAction,
        string Key,
        decimal AbsoluteDelay);

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KeyAction
    {
        Unspecified = 0,

        Press = 1,
        Release = 2
    }
}
