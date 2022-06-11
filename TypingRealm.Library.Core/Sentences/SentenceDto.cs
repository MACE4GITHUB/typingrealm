namespace TypingRealm.Library.Sentences;

/// <summary>
/// Sentence text value with it's identifier.
/// </summary>
/// <remarks>
/// Sentence identifier can be used later on by the client side to filter out or
/// remove / disable some sentences for users if they want to avoid typing them
/// again.
/// </remarks>
public sealed class SentenceDto
{
    /// <summary>
    /// Sentence unique identifier.
    /// </summary>
    /// <remarks>
    /// Later we might be able to disable select sentences.
    /// </remarks>
    public string SentenceId { get; init; } = null!;

    /// <summary>
    /// Text value of the sentence.
    /// </summary>
    public string Value { get; init; } = null!;
}
