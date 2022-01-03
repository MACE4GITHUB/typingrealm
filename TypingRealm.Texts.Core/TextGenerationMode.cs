namespace TypingRealm.Texts;

/// <summary>
/// Every mode of text generation uses separate statistics store to store statistical data.
/// </summary>
public enum TextGenerationMode
{
    Unspecified,

    /// <summary>
    /// Standard unbiased text with default length, for main statistics.
    /// </summary>
    StandardText = 1,

    /// <summary>
    /// Words with punctuation and capital letters, based on user's data, of default length.
    /// </summary>
    StandardWords = 2,

    /// <summary>
    /// Custom generated text based on <see cref="TextGenerationConfiguration"/>,
    /// statistics is stored separately.
    /// </summary>
    Custom = 3
}
