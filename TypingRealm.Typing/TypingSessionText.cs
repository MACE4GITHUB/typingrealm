namespace TypingRealm.Typing
{
    /// <summary>
    /// Entity. One typing session can have multiple texts, but every user that
    /// uses the same session should type the same amount of texts in the same
    /// consecutive order.
    /// </summary>
    public sealed record TypingSessionText(
        string TextId,
        string Value);
}
