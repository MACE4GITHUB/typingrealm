using System.Linq;

namespace TypingRealm.Messaging;

public static class ConnectedClientExtensions
{
    public static string? GetSingleGroupOrDefault(this ConnectedClient connectedClient)
    {
        return connectedClient.Groups.SingleOrDefault();
    }

    public static string GetSingleGroupOrThrow(this ConnectedClient connectedClient)
    {
        return connectedClient.Groups.Single();
    }

    public static void SetGroup(this ConnectedClient connectedClient, string value)
    {
        // TODO: Lock or make sure this operation is atomical.
        var currentGroup = connectedClient.GetSingleGroupOrThrow();
        if (currentGroup == value)
            return;

        connectedClient.RemoveFromGroup(currentGroup);
        connectedClient.AddToGroup(value);
    }
}
