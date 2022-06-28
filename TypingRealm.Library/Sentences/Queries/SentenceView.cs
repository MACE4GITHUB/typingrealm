namespace TypingRealm.Library.Sentences.Queries;

/// <summary>
/// Sentence text value with it's identifier.
/// </summary>
/// <remarks>
/// Sentence identifier can be used later on by the client side to filter out or
/// remove / disable some sentences for users if they want to avoid typing them
/// again.
/// </remarks>
public sealed record SentenceView(string SentenceId, string Value)
{
    /// <summary>
    /// Sentence unique identifier.
    /// </summary>
    /// <remarks>
    /// Later we might be able to disable select sentences.
    /// </remarks>
    public string SentenceId { get; init; } = SentenceId;

    /// <summary>
    /// Text value of the sentence.
    /// </summary>
    public string Value { get; init; } = Value;
}
