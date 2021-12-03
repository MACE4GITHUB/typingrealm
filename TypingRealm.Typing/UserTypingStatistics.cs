using System;
using System.Collections.Generic;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Cached entity used to not get all the typing sessions from the database.
    /// </summary>
    public sealed record UserTypingStatistics(
        long TextsTypedCount,
        decimal SpeedCpm,
        IEnumerable<KeyPairAggregatedData> KeyPairs,
        DateTime LastHandledResultUtc);
}
