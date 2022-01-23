namespace TypingRealm.Texts;

/// <summary>
/// Every mode of text generation uses separate statistics store to store statistical data.
/// </summary>
public enum StatisticsType
{
    Unspecified,

    /// <summary>
    /// Standard unbiased text (or words) with default length, for main statistics.
    /// No should-contains are requested.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Custom generated text based on <see cref="TextGenerationConfiguration"/>,
    /// statistics is stored separately. No should-contains are requested.
    /// </summary>
    Custom = 3,

    /// <summary>
    /// For other custom cases like short parts of texts on screen in real-time game.
    /// </summary>
    Other = 5
}
