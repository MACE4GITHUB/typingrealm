using System.Collections.Generic;

namespace TypingRealm.Messaging.Updating;

/// <summary>
/// Marks messaging groups for update. It should be thread-safe.
/// </summary>
public interface IUpdateDetector
{
    /// <summary>
    /// Mark messaging groups for update.
    /// </summary>
    /// <param name="groups">Messaging groups.</param>
    void MarkForUpdate(IEnumerable<string> groups);

    /// <summary>
    /// Returns collection of all the messaging groups that were marked for
    /// update and un-marks them.
    /// </summary>
    /// <returns>Collection of all marked messaging groups.</returns>
    IEnumerable<string> PopMarked();
}
