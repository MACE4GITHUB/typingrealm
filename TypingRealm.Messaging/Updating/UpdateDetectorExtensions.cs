namespace TypingRealm.Messaging.Updating;

public static class UpdateDetectorExtensions
{
    public static void MarkForUpdate(this IUpdateDetector updateDetector, string group)
    {
        updateDetector.MarkForUpdate(EnumerableHelpers.AsEnumerable(group));
    }
}
