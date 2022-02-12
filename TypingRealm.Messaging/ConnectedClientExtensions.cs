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
}
