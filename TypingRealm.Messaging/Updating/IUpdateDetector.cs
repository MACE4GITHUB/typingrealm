using System.Collections.Generic;

namespace TypingRealm.Messaging.Updating
{
    /// <summary>
    /// Marks messaging groups for update. It should be thread-safe.
    /// </summary>
    public interface IUpdateDetector
    {
        /// <summary>
        /// Mark messaging group for update.
        /// </summary>
        /// <param name="group">Messaging group.</param>
        void MarkForUpdate(string group);

        /// <summary>
        /// Returns collection of all the messaging groups that were marked for
        /// update and un-marks them.
        /// </summary>
        /// <returns>Collection of all marked messaging groups.</returns>
        IEnumerable<string> PopMarked();
    }
}
