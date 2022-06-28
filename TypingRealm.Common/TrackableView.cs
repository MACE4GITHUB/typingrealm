using System;

namespace TypingRealm;

// This is a temporary class for showcase purposes. Domain shouldn't have
// tracking concern embedded into it.

/// <summary>
/// Technical view that returns technical data about creating and updating.
/// </summary>
public abstract class TrackableView
{
    /// <summary>
    /// Shows UTC time when the entity has been created.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Shows UTC time when the entity has been last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; init; }

    /// <summary>
    /// Shows the user (profile identifier) who created the entity.
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Shows the user (profile identifier) who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; init; }
}
